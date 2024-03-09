using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.BitSets;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Utility;

namespace Model.Sudoku.Solver.Strategies;

public class ExtendedUniqueRectanglesStrategy : SudokuStrategy
{
    public const string OfficialName = "Extended Unique Rectangles";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public ExtendedUniqueRectanglesStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }
    
    public override void Apply(IStrategyUser strategyUser)
    {
        strategyUser.PreComputer.Graphs.ConstructSimple(ConstructRule.CellStrongLink, ConstructRule.UnitStrongLink,
            ConstructRule.CellWeakLink, ConstructRule.UnitWeakLink);
        
        for (int mini = 0; mini < 3; mini++)
        {
            if (Search(strategyUser, mini, Unit.Row)) return;
            if (Search(strategyUser, mini, Unit.Column)) return;
        }
    }

    private bool Search(IStrategyUser strategyUser, int mini, Unit unit)
    {
        for (int u = 0; u < 3; u++)
        {
            for (int miniO1 = 0; miniO1 < 2; miniO1++)
            {
                var o1 = mini * 3 + miniO1;
                var c1 = unit == Unit.Row ? new Cell(o1, u) : new Cell( u, o1);
                var p1 = strategyUser.PossibilitiesAt(c1);
                if (p1.Count == 0) continue;
                
                for (int miniO2 = miniO1 + 1; miniO2 < 3; miniO2++)
                {
                    var o2 = mini * 3 + miniO2;
                    var c2 = unit == Unit.Row ? new Cell(o2, u) : new Cell(u, o2);
                    var p2 = strategyUser.PossibilitiesAt(c2);
                    if (p2.Count == 0 || !p1.ContainsAll(p2)) continue;

                    if (ContinueSearch(strategyUser, unit, c1, c2, p1 | p2)) return true;
                }
            }
        }
        
        return false;
    }

    private bool ContinueSearch(IStrategyUser strategyUser, Unit unit, Cell c1, Cell c2, ReadOnlyBitSet16 poss)
    {
        var list = new List<Cell> { c1, c2 };

        for (int u = 3; u < 6; u++)
        {
            var c3 = unit == Unit.Row ? new Cell(c1.Row, u) : new Cell(u, c1.Column);
            var p3 = strategyUser.PossibilitiesAt(c3);
            if (p3.Count == 0 || !poss.ContainsAny(p3)) continue;
            
            var c4 = unit == Unit.Row ? new Cell(c2.Row, u) : new Cell(u, c2.Column);
            var p4 = strategyUser.PossibilitiesAt(c4);
            if (p4.Count == 0 || !poss.ContainsAny(p3) || !p4.ContainsAny(p3)) continue;

            list.Add(c3);
            list.Add(c4);

            for (int w = 6; w < 9; w++)
            {
                var c5 = unit == Unit.Row ? new Cell(c1.Row, w) : new Cell(w, c1.Column);
                var p5 = strategyUser.PossibilitiesAt(c5);
                if (p5.Count == 0 || !poss.ContainsAny(p5)) continue;
            
                var c6 = unit == Unit.Row ? new Cell(c2.Row, w) : new Cell(w, c2.Column);
                var p6 = strategyUser.PossibilitiesAt(c6);
                if (p6.Count == 0 || !poss.ContainsAny(p6) || !p6.ContainsAny(p5)) continue;

                list.Add(c5);
                list.Add(c6);

                if (ProcessCombinations(strategyUser, poss.OrMulti(p3, p4, p5, p6), list)) return true;
                list.RemoveRange(list.Count - 2, 2);
            }

            list.RemoveRange(list.Count - 2, 2);
        }

        return false;
    }

    private bool ProcessCombinations(IStrategyUser strategyUser, ReadOnlyBitSet16 poss, List<Cell> cells)
    {
        if (poss.Count < 3) return false;
        var array = poss.ToArray();

        foreach (var combination in CombinationCalculator.EveryCombinationWithSpecificCount(3, array))
        {
            if (Process(strategyUser, new ReadOnlyBitSet16(combination), cells)) return true;
        }
        
        return false;
    }

    private bool Process(IStrategyUser strategyUser, ReadOnlyBitSet16 poss, List<Cell> cells) //TODO to general method like "ProcessMustBeTrue"
    {
        List<CellPossibility> pNotInPattern = new List<CellPossibility>();
        List<Cell> cNotInPattern = new List<Cell>();
        var graph = strategyUser.PreComputer.Graphs.SimpleLinkGraph;
        
        foreach (var cell in cells)
        {
            var p = strategyUser.PossibilitiesAt(cell) - poss;
            if (p.Count == 0) continue;

            cNotInPattern.Add(cell);
            foreach (var possibility in p.EnumeratePossibilities())
            {
                pNotInPattern.Add(new CellPossibility(cell, possibility));
            }
        }

        if (cNotInPattern.Count == 0) return false;

        if (cNotInPattern.Count == 1)
        {
            foreach (var p in poss.EnumeratePossibilities())
            {
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(p, cNotInPattern[0].Row, cNotInPattern[0].Column);
            }
            
            return strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                       new ExtendedUniqueRectanglesReportBuilder(poss, cells.ToArray())) &&
                   OnCommitBehavior == OnCommitBehavior.Return;
        }
        
        
        if (cNotInPattern.Count == 2)
        {
            foreach (var p in poss.EnumeratePossibilities())
            {
                var cp1 = new CellPossibility(cNotInPattern[0], p);
                var cp2 = new CellPossibility(cNotInPattern[1], p);
                if (!graph.AreNeighbors(cp1, cp2, LinkStrength.Strong)) continue;
                
                foreach (var elimination in poss.EnumeratePossibilities())
                {
                    if (elimination == p) continue;
                    strategyUser.ChangeBuffer.ProposePossibilityRemoval(elimination, cNotInPattern[0]);
                    strategyUser.ChangeBuffer.ProposePossibilityRemoval(elimination, cNotInPattern[1]);
                }
            }
        }
            
        foreach (var target in graph.Neighbors(pNotInPattern[0]))
        {
            bool ok = true;
            for (int i = 1; i < pNotInPattern.Count; i++)
            {
                if (!graph.AreNeighbors(pNotInPattern[i], target))
                {
                    ok = false;
                    break;
                }
            }

            if (ok) strategyUser.ChangeBuffer.ProposePossibilityRemoval(target);
        }
        

        return strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                   new ExtendedUniqueRectanglesReportBuilder(poss, cells.ToArray())) &&
                        OnCommitBehavior == OnCommitBehavior.Return;
    }
}

public class ExtendedUniqueRectanglesReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState>
{
    private readonly ReadOnlyBitSet16 _poss;
    private readonly Cell[] _cells;

    public ExtendedUniqueRectanglesReportBuilder(ReadOnlyBitSet16 poss, Cell[] cells)
    {
        _poss = poss;
        _cells = cells;
    }

    public ChangeReport Build(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport( "", lighter =>
        {
            foreach (var cell in _cells)
            {
                lighter.HighlightCell(cell, _poss.ContainsAll(snapshot.PossibilitiesAt(cell))
                    ? ChangeColoration.CauseOffOne 
                    : ChangeColoration.CauseOffTwo);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
}