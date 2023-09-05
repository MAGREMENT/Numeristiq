using System.Collections.Generic;
using Model.Changes;
using Model.Positions;
using Model.Solver;
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
                var ppic = strategyManager.RowPositions(row, number);
                if (ppic.Count == 0) continue;

                var here = new LinePositions { row };

                SearchRowCandidate(strategyManager, row + 1, ppic, here, number);
            }
            
            for (int col = 0; col < 9; col++)
            {
                var ppir = strategyManager.ColumnPositions(col, number);
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
            var ppic = strategyManager.RowPositions(row, number);

            var newMashed = mashed.Mash(ppic);
            if (newMashed.Count > _type || newMashed.Count == mashed.Count + ppic.Count) continue;
            
            var newVisited = visited.Copy();
            newVisited.Add(row);

            if (newVisited.Count == newMashed.Count - 1 && newMashed.Count == _type)
                SearchRowFinned(strategyManager, newMashed, newVisited, number);
            else if(newVisited.Count < _type) SearchRowCandidate(strategyManager, row + 1, newMashed, newVisited, number);
        }
    }

    private void SearchRowFinned(IStrategyManager strategyManager, LinePositions mashed, LinePositions visited,
        int number)
    {
        for (int row = 0; row < 9; row++)
        {
            if (visited.Peek(row)) continue;

            var ppic = strategyManager.RowPositions(row, number);

            int miniCol = -1;

            foreach (var col in ppic)
            {
                if (mashed.Peek(col)) continue;
                if (miniCol == -1) miniCol = col / 3;
                else if(col / 3 != miniCol)
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
                    if (visited.Peek(eliminationRow) || row == eliminationRow) continue;

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
            var ppir = strategyManager.ColumnPositions(col, number);

            var newMashed = mashed.Mash(ppir);
            if (newMashed.Count > _type || newMashed.Count == mashed.Count + ppir.Count) continue;

            var newVisited = visited.Copy();
            newVisited.Add(col);

            if (newVisited.Count == newMashed.Count - 1 && newMashed.Count == _type)
                SearchColumnFinned(strategyManager, newMashed, newVisited, number);
            else if(newVisited.Count < _type) SearchColumnCandidate(strategyManager, col + 1, newMashed, newVisited, number);
        }
    }
    
    private void SearchColumnFinned(IStrategyManager strategyManager, LinePositions mashed, LinePositions visited,
        int number)
    {
        for (int col = 0; col < 9; col++)
        {
            if (visited.Peek(col)) continue;

            var ppic = strategyManager.ColumnPositions(col, number);

            int miniRow = -1;

            foreach (var row in ppic)
            {
                if (mashed.Peek(row)) continue;
                if (miniRow == -1) miniRow = row / 3;
                else if(row / 3 != miniRow)
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
                    if (visited.Peek(eliminationCol) || col == eliminationCol) continue;

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
                int row = _unit == Unit.Row ? visited : mashed;
                int col = _unit == Unit.Row ? mashed : visited;

                if (manager.Possibilities[row, col].Peek(_number)) normal.Add(new Coordinate(row, col));
            }
        }

        for (int n = 0; n < 9; n++)
        {
            int row = _unit == Unit.Row ? _fin : n;
            int col = _unit == Unit.Row ? n : _fin;

            if (manager.Possibilities[row, col].Peek(_number))
            {
                if (_mashed.Peek(n)) normal.Add(new Coordinate(row, col));
                else finned.Add(new Coordinate(row, col));
            }
        }
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "",
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
            });
    }
}