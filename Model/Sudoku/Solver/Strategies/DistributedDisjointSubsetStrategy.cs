using System.Collections.Generic;
using Model.Sudoku.Solver.Helpers.Changes;
using Model.Sudoku.Solver.Position;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;

namespace Model.Sudoku.Solver.Strategies;

public class DistributedDisjointSubsetStrategy : AbstractStrategy
{
    public const string OfficialName = "Distributed Disjoint Subset";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public DistributedDisjointSubsetStrategy() : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
    }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        HashSet<GridPositions> alreadyExplored = new();
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Sudoku[row, col] != 0) continue;

                var current = new Cell(row, col);
                
                GridPositions positions = new GridPositions();
                positions.Add(row, col);
                Dictionary<int, List<Cell>> possibilitiesCells = new();
                foreach (var p in strategyManager.PossibilitiesAt(row, col))
                {
                    possibilitiesCells.Add(p, new List<Cell>{current});
                }

                if (Search(strategyManager, possibilitiesCells, positions, alreadyExplored)) return;
            }
        }
    }

    private bool Search(IStrategyManager strategyManager, Dictionary<int, List<Cell>> possibilitiesCells,
        GridPositions positions, HashSet<GridPositions> alreadyExplored)
    {
        foreach (var cell in positions.AllSeenCells())
        {
            if (strategyManager.Sudoku[cell.Row, cell.Column] != 0 ||
                !ShareAUnitWithAll(strategyManager, cell, possibilitiesCells)) continue;
            
            positions.Add(cell);
            if (alreadyExplored.Contains(positions))
            {
                positions.Remove(cell);
                continue;
            }

            alreadyExplored.Add(positions.Copy());
            
            foreach (var p in strategyManager.PossibilitiesAt(cell))
            {
                if (!possibilitiesCells.TryGetValue(p, out var list))
                {
                    list = new List<Cell>();
                    possibilitiesCells[p] = list;
                }

                list.Add(cell);
            }

            if (positions.Count == possibilitiesCells.Count)
            {
                if (Process(strategyManager, possibilitiesCells)) return true;
            }
            
            if (Search(strategyManager, possibilitiesCells, positions, alreadyExplored)) return true;

            positions.Remove(cell);
            foreach (var p in strategyManager.PossibilitiesAt(cell))
            {
                var list = possibilitiesCells[p];

                list.RemoveAt(list.Count - 1);
                if (list.Count == 0) possibilitiesCells.Remove(p);
            }
        }
        
        return false;
    }

    private bool Process(IStrategyManager strategyManager, Dictionary<int, List<Cell>> possibilitiesCells)
    {
        foreach (var entry in possibilitiesCells)
        {
            foreach (var ssc in Cells.SharedSeenCells(entry.Value))
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key, ssc);
            }
        }

        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                   new DistributedDisjointSubsetReportBuilder(PossibilitiesCellsDeepCopy(possibilitiesCells))) &&
               OnCommitBehavior == OnCommitBehavior.Return;
    }

    private Dictionary<int, List<Cell>> PossibilitiesCellsDeepCopy(Dictionary<int, List<Cell>> original)
    {
        var result = new Dictionary<int, List<Cell>>();

        foreach (var entry in original)
        {
            result.Add(entry.Key, new List<Cell>(entry.Value));
        }
        
        return result;
    }

    private bool ShareAUnitWithAll(IStrategyManager strategyManager, Cell cell, Dictionary<int, List<Cell>> possibilitiesCells)
    {
        bool ok = false;
        foreach (var poss in strategyManager.PossibilitiesAt(cell))
        {
            if (!possibilitiesCells.TryGetValue(poss, out var toShareAUnitWith)) continue;

            ok = true;
            foreach (var c in toShareAUnitWith)
            {
                if (!Cells.ShareAUnit(cell, c)) return false;
            }
        }

        return ok;
    }
}

public class DistributedDisjointSubsetReportBuilder : IChangeReportBuilder
{
    private readonly Dictionary<int, List<Cell>> _possibilitiesCells;

    public DistributedDisjointSubsetReportBuilder(Dictionary<int, List<Cell>> possibilitiesCells)
    {
        _possibilitiesCells = possibilitiesCells;
    }

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            int color = (int)ChangeColoration.CauseOffOne;
            foreach (var entry in _possibilitiesCells)
            {
                foreach (var cell in entry.Value)
                {
                    lighter.HighlightPossibility(entry.Key, cell.Row, cell.Column, (ChangeColoration)color);
                }

                color++;
            }
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}