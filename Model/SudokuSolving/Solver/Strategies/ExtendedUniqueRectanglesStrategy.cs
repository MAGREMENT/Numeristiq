using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.SudokuSolving.Solver.Helpers.Changes;
using Model.SudokuSolving.Solver.Possibility;
using Model.SudokuSolving.Solver.StrategiesUtility;
using Model.SudokuSolving.Solver.StrategiesUtility.Graphs;

namespace Model.SudokuSolving.Solver.Strategies;

public class ExtendedUniqueRectanglesStrategy : AbstractStrategy
{
    public const string OfficialName = "Extended Unique Rectangles";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public ExtendedUniqueRectanglesStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        strategyManager.GraphManager.ConstructSimple(ConstructRule.CellStrongLink, ConstructRule.UnitStrongLink,
            ConstructRule.CellWeakLink, ConstructRule.UnitWeakLink);
        
        for (int mini = 0; mini < 3; mini++)
        {
            if (Search(strategyManager, mini, Unit.Row)) return;
            if (Search(strategyManager, mini, Unit.Column)) return;
        }
    }

    private bool Search(IStrategyManager strategyManager, int mini, Unit unit)
    {
        for (int u = 0; u < 3; u++)
        {
            for (int miniO1 = 0; miniO1 < 2; miniO1++)
            {
                var o1 = mini * 3 + miniO1;
                var c1 = unit == Unit.Row ? new Cell(o1, u) : new Cell( u, o1);
                var p1 = strategyManager.PossibilitiesAt(c1);
                if (p1.Count == 0) continue;
                
                for (int miniO2 = miniO1 + 1; miniO2 < 3; miniO2++)
                {
                    var o2 = mini * 3 + miniO2;
                    var c2 = unit == Unit.Row ? new Cell(o2, u) : new Cell(u, o2);
                    var p2 = strategyManager.PossibilitiesAt(c2);
                    if (p2.Count == 0 || !p1.PeekAll(p2)) continue;

                    if (ContinueSearch(strategyManager, unit, c1, c2, p1.Or(p2))) return true;
                }
            }
        }
        
        return false;
    }

    private bool ContinueSearch(IStrategyManager strategyManager, Unit unit, Cell c1, Cell c2, Possibilities poss)
    {
        var list = new List<Cell> { c1, c2 };

        for (int u = 3; u < 6; u++)
        {
            var c3 = unit == Unit.Row ? new Cell(c1.Row, u) : new Cell(u, c1.Column);
            var p3 = strategyManager.PossibilitiesAt(c3);
            if (p3.Count == 0 || !poss.PeekAny(p3)) continue;
            
            var c4 = unit == Unit.Row ? new Cell(c2.Row, u) : new Cell(u, c2.Column);
            var p4 = strategyManager.PossibilitiesAt(c4);
            if (p4.Count == 0 || !poss.PeekAny(p3) || !p4.PeekAny(p3)) continue;

            list.Add(c3);
            list.Add(c4);

            for (int w = 6; w < 9; w++)
            {
                var c5 = unit == Unit.Row ? new Cell(c1.Row, w) : new Cell(w, c1.Column);
                var p5 = strategyManager.PossibilitiesAt(c5);
                if (p5.Count == 0 || !poss.PeekAny(p5)) continue;
            
                var c6 = unit == Unit.Row ? new Cell(c2.Row, w) : new Cell(w, c2.Column);
                var p6 = strategyManager.PossibilitiesAt(c6);
                if (p6.Count == 0 || !poss.PeekAny(p6) || !p6.PeekAny(p5)) continue;

                list.Add(c5);
                list.Add(c6);

                if (ProcessCombinations(strategyManager, poss.Or(p3, p4, p5, p6), list)) return true;
                list.RemoveRange(list.Count - 2, 2);
            }

            list.RemoveRange(list.Count - 2, 2);
        }

        return false;
    }

    private bool ProcessCombinations(IStrategyManager strategyManager, Possibilities poss, List<Cell> cells)
    {
        if (poss.Count < 3) return false;
        var array = poss.ToArray();

        foreach (var combination in CombinationCalculator.EveryCombinationWithSpecificCount(3, array))
        {
            if (Process(strategyManager, Possibilities.FromEnumerable(combination), cells)) return true;
        }
        
        return false;
    }

    private bool Process(IStrategyManager strategyManager, Possibilities poss, List<Cell> cells) //TODO to general method like "ProcessMustBeTrue"
    {
        List<CellPossibility> pNotInPattern = new List<CellPossibility>();
        List<Cell> cNotInPattern = new List<Cell>();
        var graph = strategyManager.GraphManager.SimpleLinkGraph;
        
        foreach (var cell in cells)
        {
            var p = strategyManager.PossibilitiesAt(cell).Difference(poss);
            if (p.Count == 0) continue;

            cNotInPattern.Add(cell);
            foreach (var possibility in p)
            {
                pNotInPattern.Add(new CellPossibility(cell, possibility));
            }
        }

        if (cNotInPattern.Count == 0) return false;

        if (cNotInPattern.Count == 1)
        {
            foreach (var p in poss)
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, cNotInPattern[0].Row, cNotInPattern[0].Column);
            }
            
            return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                       new ExtendedUniqueRectanglesReportBuilder(poss, cells.ToArray())) &&
                   OnCommitBehavior == OnCommitBehavior.Return;
        }
        
        
        if (cNotInPattern.Count == 2)
        {
            foreach (var p in poss)
            {
                var cp1 = new CellPossibility(cNotInPattern[0], p);
                var cp2 = new CellPossibility(cNotInPattern[1], p);
                if (!graph.AreNeighbors(cp1, cp2, LinkStrength.Strong)) continue;
                
                foreach (var elimination in poss)
                {
                    if (elimination == p) continue;
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(elimination, cNotInPattern[0]);
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(elimination, cNotInPattern[1]);
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

            if (ok) strategyManager.ChangeBuffer.ProposePossibilityRemoval(target);
        }
        

        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                   new ExtendedUniqueRectanglesReportBuilder(poss, cells.ToArray())) &&
                        OnCommitBehavior == OnCommitBehavior.Return;
    }
}

public class ExtendedUniqueRectanglesReportBuilder : IChangeReportBuilder
{
    private readonly Possibilities _poss;
    private readonly Cell[] _cells;

    public ExtendedUniqueRectanglesReportBuilder(Possibilities poss, Cell[] cells)
    {
        _poss = poss;
        _cells = cells;
    }

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var cell in _cells)
            {
                lighter.HighlightCell(cell, _poss.PeekAll(snapshot.PossibilitiesAt(cell))
                    ? ChangeColoration.CauseOffOne 
                    : ChangeColoration.CauseOffTwo);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}