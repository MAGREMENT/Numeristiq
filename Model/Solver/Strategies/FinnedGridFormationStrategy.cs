using System;
using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Positions;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

public class FinnedGridFormationStrategy : AbstractStrategy
{
    public const string OfficialNameForType3 = "Finned Swordfish";
    public const string OfficialNameForType4 = "Finned Jellyfish";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly int _type;

    public FinnedGridFormationStrategy(int type) : base("", StrategyDifficulty.Hard, DefaultBehavior)
    {
        _type = type;
        Name = type switch
        {
            3 => OfficialNameForType3,
            4 => OfficialNameForType4,
            _ => throw new ArgumentException("Type not valid")
        };
    }
    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var ppic = strategyManager.RowPositionsAt(row, number);
                if (ppic.Count == 0) continue;

                var here = new LinePositions { row };

                if(SearchRowCandidate(strategyManager, row + 1, ppic, here, number)) return;
            }
            
            for (int col = 0; col < 9; col++)
            {
                var ppir = strategyManager.ColumnPositionsAt(col, number);
                if (ppir.Count == 0) continue;

                var here = new LinePositions { col };

                if (SearchColumnCandidate(strategyManager, col + 1, ppir, here, number)) return;
            }
        }
    }

    private bool SearchRowCandidate(IStrategyManager strategyManager, int start, IReadOnlyLinePositions mashed,
        LinePositions visited, int number)
    {
        for (int row = start; row < 9; row++)
        {
            var ppic = strategyManager.RowPositionsAt(row, number);
            if (ppic.Count > _type) continue;

            var newMashed = mashed.Or(ppic);
            if (newMashed.Count > _type || newMashed.Count == mashed.Count + ppic.Count) continue;
            
            var newVisited = visited.Copy();
            newVisited.Add(row);

            if (newVisited.Count == newMashed.Count - 1 && newMashed.Count == _type)
            {
                if (SearchRowFinned(strategyManager, newMashed, newVisited, number)) return true;
            }
            else if(newVisited.Count < _type) SearchRowCandidate(strategyManager, row + 1, newMashed, newVisited, number);
        }

        return false;
    }

    private bool SearchRowFinned(IStrategyManager strategyManager, IReadOnlyLinePositions mashed, LinePositions visited,
        int number)
    {
        for (int row = 0; row < 9; row++)
        {
            if (visited.Peek(row)) continue;

            var ppic = strategyManager.RowPositionsAt(row, number);

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

            if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                    new FinnedGridFormationReportBuilder(mashed, visited, row, number, Unit.Row)) &&
                    OnCommitBehavior == OnCommitBehavior.Return) return true;
        }

        return false;
    }
    
    private bool SearchColumnCandidate(IStrategyManager strategyManager, int start, IReadOnlyLinePositions mashed,
        LinePositions visited, int number)
    {
        for (int col = start; col < 9; col++)
        {
            var ppir = strategyManager.ColumnPositionsAt(col, number);
            if(ppir.Count > _type) continue;

            var newMashed = mashed.Or(ppir);
            if (newMashed.Count > _type || newMashed.Count == mashed.Count + ppir.Count) continue;

            var newVisited = visited.Copy();
            newVisited.Add(col);

            if (newVisited.Count == newMashed.Count - 1 && newMashed.Count == _type)
            {
                if (SearchColumnFinned(strategyManager, newMashed, newVisited, number)) return true;
            }
            else if(newVisited.Count < _type) SearchColumnCandidate(strategyManager, col + 1, newMashed, newVisited, number);
        }

        return false;
    }
    
    private bool SearchColumnFinned(IStrategyManager strategyManager, IReadOnlyLinePositions mashed, LinePositions visited,
        int number)
    {
        for (int col = 0; col < 9; col++)
        {
            if (visited.Peek(col)) continue;

            var ppic = strategyManager.ColumnPositionsAt(col, number);

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

            if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                    new FinnedGridFormationReportBuilder(mashed, visited, col, number, Unit.Column))
                    && OnCommitBehavior == OnCommitBehavior.Return) return true;
        }

        return false;
    }
}

public class FinnedGridFormationReportBuilder : IChangeReportBuilder
{
    private readonly IReadOnlyLinePositions _mashed;
    private readonly IReadOnlyLinePositions _visited;
    private readonly int _fin;
    private readonly int _number;
    private readonly Unit _unit;

    public FinnedGridFormationReportBuilder(IReadOnlyLinePositions mashed, IReadOnlyLinePositions visited, int fin, int number, Unit unit)
    {
        _mashed = mashed;
        _visited = visited;
        _fin = fin;
        _unit = unit;
        _number = number;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        List<Cell> normal = new();
        List<Cell> finned = new();

        foreach (var visited in _visited)
        {
            foreach (var mashed in _mashed)
            {
                int row = _unit == Unit.Row ? visited : mashed;
                int col = _unit == Unit.Row ? mashed : visited;

                if (snapshot.PossibilitiesAt(row, col).Peek(_number)) normal.Add(new Cell(row, col));
            }
        }

        for (int n = 0; n < 9; n++)
        {
            int row = _unit == Unit.Row ? _fin : n;
            int col = _unit == Unit.Row ? n : _fin;

            if (snapshot.PossibilitiesAt(row, col).Peek(_number))
            {
                if (_mashed.Peek(n)) normal.Add(new Cell(row, col));
                else finned.Add(new Cell(row, col));
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