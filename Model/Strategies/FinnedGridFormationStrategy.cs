using System.Collections.Generic;
using Model.Positions;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class FinnedGridFormationStrategy : IStrategy
{
    public string Name { get; }
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public int Score { get; set; }
    
    private readonly int _type;

    public FinnedGridFormationStrategy(int type)
    {
        _type = type;
        Name = type switch
        {
            3 => "Finned Swordfish",
            4 => "Finned Jellyfish",
            _ => "Finned Grid formation unknown"
        };
    }
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var ppic = strategyManager.PossibilityPositionsInRow(row, number);
                if (ppic.Count == 0) continue;

                var here = new LinePositions { row };

                SearchRowCandidate(strategyManager, row + 1, ppic, here, number);
            }
            
            for (int col = 0; col < 9; col++)
            {
                var ppir = strategyManager.PossibilityPositionsInColumn(col, number);
                if (ppir.Count == 0) continue;

                var here = new LinePositions { col };

                SearchColumnCandidate(strategyManager, col + 1, ppir, here, number);
            }
        }
    }

    private void SearchRowCandidate(IStrategyManager strategyManager, int start, LinePositions mashed,
        LinePositions visited, int number)
    {
        for (int row = start; row < 9; row++)
        {
            var ppic = strategyManager.PossibilityPositionsInRow(row, number);

            var newMashed = mashed.Mash(ppic);
            if (newMashed.Count > _type || newMashed.Count == mashed.Count + ppic.Count) continue;

            if (visited.Count == newMashed.Count - 1 && newMashed.Count == _type)
                SearchRowFinned(strategyManager, newMashed, visited, number);
        }
    }

    private void SearchRowFinned(IStrategyManager strategyManager, LinePositions mashed, LinePositions visited,
        int number)
    {
        for (int row = 0; row < 9; row++)
        {
            if (visited.Peek(row)) continue;

            var ppic = strategyManager.PossibilityPositionsInRow(row, number);

            int miniCol = -1;

            foreach (var col in ppic)
            {
                if (mashed.Peek(col)) continue;
                if (miniCol == -1) miniCol = col;
                else
                {
                    miniCol = -1;
                    break;
                }
            }

            if (miniCol == -1) continue;
            
            foreach (var col in mashed)
            {
                if (col / 3 != miniCol) continue;
                int startRow = row / 3 * 3;

                for (int gridRow = 0; gridRow < 3; gridRow++)
                {
                    int eliminationRow = startRow + gridRow;
                    if (visited.Peek(eliminationRow)) continue;

                    strategyManager.ChangeBuffer.AddPossibilityToRemove(number, eliminationRow, col);
                }
            }

            if (strategyManager.ChangeBuffer.NotEmpty())
                strategyManager.ChangeBuffer.Push(this,
                    new FinnedGridFormationReportBuilder(mashed, visited, row, number, Unit.Row));

        }
    }
    
    private void SearchColumnCandidate(IStrategyManager strategyManager, int start, LinePositions mashed,
        LinePositions visited, int number)
    {
        for (int col = start; col < 9; col++)
        {
            var ppir = strategyManager.PossibilityPositionsInColumn(col, number);

            var newMashed = mashed.Mash(ppir);
            if (newMashed.Count > _type || newMashed.Count == mashed.Count + ppir.Count) continue;

            if (visited.Count == newMashed.Count - 1 && newMashed.Count == _type)
                SearchColumnFinned(strategyManager, newMashed, visited, number);
        }
    }
    
    private void SearchColumnFinned(IStrategyManager strategyManager, LinePositions mashed, LinePositions visited,
        int number)
    {
        for (int col = 0; col < 9; col++)
        {
            if (visited.Peek(col)) continue;

            var ppir = strategyManager.PossibilityPositionsInRow(col, number);

            int miniRow = -1;

            foreach (var row in ppir)
            {
                if (mashed.Peek(col)) continue;
                if (miniRow == -1) miniRow = row;
                else
                {
                    miniRow = -1;
                    break;
                }
            }

            if (miniRow  == -1) continue;
            
            foreach (var row in mashed)
            {
                if (row / 3 != miniRow) continue;
                int startCol = col / 3 * 3;

                for (int gridCol = 0; gridCol < 3; gridCol++)
                {
                    int eliminationCol = startCol + gridCol;
                    if (visited.Peek(eliminationCol)) continue;

                    strategyManager.ChangeBuffer.AddPossibilityToRemove(number, row, eliminationCol);
                }
            }

            if (strategyManager.ChangeBuffer.NotEmpty())
                strategyManager.ChangeBuffer.Push(this,
                    new FinnedGridFormationReportBuilder(mashed, visited, col, number, Unit.Column));

        }
    }
}

public class FinnedGridFormationReportBuilder : IChangeReportBuilder
{
    private readonly LinePositions _mashed;
    private readonly LinePositions _visited;
    private readonly int _fin;
    private readonly int _number;
    private readonly Unit _unit;

    public FinnedGridFormationReportBuilder(LinePositions mashed, LinePositions visited, int fin, int number, Unit unit)
    {
        _mashed = mashed;
        _visited = visited;
        _fin = fin;
        _unit = unit;
        _number = number;
    }

    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        List<Coordinate> normal = new();
        List<Coordinate> finned = new();

        foreach (var visited in _visited)
        {
            foreach (var mashed in _mashed)
            {
                int row = _unit == Unit.Row ? mashed : visited;
                int col = _unit == Unit.Column ? visited : mashed;

                if (manager.Possibilities[row, col].Peek(_number)) normal.Add(new Coordinate(row, col));
            }
        }

        for (int n = 0; n < 9; n++)
        {
            int row = _unit == Unit.Row ? n : _fin;
            int col = _unit == Unit.Column ? _fin : n;

            if (manager.Possibilities[row, col].Peek(_number))
            {
                if (_mashed.Peek(n)) normal.Add(new Coordinate(row, col));
                else finned.Add(new Coordinate(row, col));
            }
        }
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes),
            lighter =>
            {
                foreach (var coord in normal)
                {
                    lighter.HighlightPossibility(_number, coord.Row, coord.Col, ChangeColoration.CauseOffOne);
                }
                
                foreach (var coord in finned)
                {
                    lighter.HighlightPossibility(_number, coord.Row, coord.Col, ChangeColoration.CauseOffTwo);
                }
                
                IChangeReportBuilder.HighlightChanges(lighter, changes);
            }, "");
    }
}