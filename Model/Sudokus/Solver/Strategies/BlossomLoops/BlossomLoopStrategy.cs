using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs.ConstructRules;
using Model.Utility;
using Model.Utility.BitSets;
using Model.Utility.Collections;

namespace Model.Sudokus.Solver.Strategies.BlossomLoops;

public class BlossomLoopStrategy : SudokuStrategy
{
    public const string OfficialNameForCell = "Cell Blossom Loop";
    public const string OfficialNameForUnit = "Unit Blossom Loop";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly IBlossomLoopType _type;
    private readonly IBlossomLoopLoopFinder _loopFinder;
    private readonly IBlossomLoopBranchFinder _branchFinder;
    
    public BlossomLoopStrategy(IBlossomLoopLoopFinder loopFinder, IBlossomLoopBranchFinder branchFinder, IBlossomLoopType type)
        : base(type.Name, Difficulty.Extreme, DefaultInstanceHandling)
    {
        _loopFinder = loopFinder;
        _branchFinder = branchFinder;
        _type = type;
    }

    
    public override void Apply(ISudokuSolverData solverData)
    {
        solverData.PreComputer.ComplexGraph.Construct(CellStrongLinkConstructionRule.Instance, CellWeakLinkConstructionRule.Instance,
            UnitStrongLinkConstructionRule.Instance, UnitWeakLinkConstructionRule.Instance,
            PointingPossibilitiesConstructionRule.Instance);
        var graph = solverData.PreComputer.ComplexGraph.Graph;

        foreach (var cps in _type.Candidates(solverData))
        {
            foreach (var loop in _loopFinder.Find(cps, graph))
            {
                var branches = _branchFinder.FindShortestBranches(graph, cps, loop);
                if (branches is null) continue;

                var nope = SetUpNope(loop, branches);
                
                loop.ForEachLink((one, two) => HandleWeakLoopLink(solverData,
                    one, two, nope, branches), LinkStrength.Weak);

                foreach (var b in branches)
                {
                    for (int i = 0; i < b.Branch.Links.Count; i++)
                    {
                        if(b.Branch.Links[i] == LinkStrength.Weak) HandleWeakBranchLink(solverData,
                            b.Branch.Elements[i], b.Branch.Elements[i + 1], nope);
                    }
                }

                if (solverData.ChangeBuffer.NeedCommit())
                {
                    solverData.ChangeBuffer.Commit(new BlossomLoopReportBuilder(loop, branches, cps));
                    if (StopOnFirstCommit) return;
                }
            }
        }
    }

    private HashSet<CellPossibility> SetUpNope(Loop<ISudokuElement, LinkStrength> loop,
        BlossomLoopBranch[] branches)
    {
        HashSet<CellPossibility> nope = new();
        foreach (var b in branches)
        {
            foreach (var element in b.Branch.Elements)
            {
                foreach (var cp in element.EnumerateCellPossibility()) nope.Add(cp);
            }
        }
        
        foreach (var element in loop)
        {
            foreach (var cp in element.EnumerateCellPossibility()) nope.Add(cp);
        }

        return nope;
    }

    private void HandleWeakLoopLink(ISudokuSolverData solverData, ISudokuElement one, ISudokuElement two,
        HashSet<CellPossibility> nope, BlossomLoopBranch[] branches)
    {
        List<ISudokuElement> toTakeIntoAccount = new();
        foreach (var b in branches)
        {
            if (one.Equals(b.Targets[0]) && two.Equals(b.Targets[1])) toTakeIntoAccount.Add(b.Branch.Elements[^1]);
        }

        if (toTakeIntoAccount.Count == 0) HandleWeakBranchLink(solverData, one, two, nope);
        else
        {
            var and = one.EveryPossibilities() & two.EveryPossibilities();
            var or = one.EveryPossibilities() | two.EveryPossibilities();
            var cells = new HashSet<Cell>(one.EnumerateCells());
            cells.UnionWith(two.EnumerateCells());

            foreach (var element in toTakeIntoAccount)
            {
                and &= element.EveryPossibilities();
                or |= element.EveryPossibilities();
                cells.UnionWith(element.EnumerateCells());
            }

            if (cells.Count == 1)
            {
                var c = cells.First();
                foreach (var p in solverData.PossibilitiesAt(c).EnumeratePossibilities())
                {
                    if (!or.Contains(p)) solverData.ChangeBuffer.ProposePossibilityRemoval(p, c);
                }
            }

            foreach (var p in and.EnumeratePossibilities())
            {
                List<Cell> c = new();
                
                foreach (var cp in one.EnumerateCellPossibilities())
                {
                    if(cp.Possibilities.Contains(p)) c.Add(cp.Cell);
                }
                
                foreach (var cp in two.EnumerateCellPossibilities())
                {
                    if(cp.Possibilities.Contains(p)) c.Add(cp.Cell);
                }

                foreach (var element in toTakeIntoAccount)
                {
                    foreach (var cp in element.EnumerateCellPossibilities())
                    {
                        if(cp.Possibilities.Contains(p)) c.Add(cp.Cell);
                    }
                }

                foreach (var ssc in SudokuUtility.SharedSeenCells(c))
                {
                    solverData.ChangeBuffer.ProposePossibilityRemoval(p, ssc);
                }
            }
        }
        
    }

    private void HandleWeakBranchLink(ISudokuSolverData solverData, ISudokuElement one, ISudokuElement two,
        HashSet<CellPossibility> nope)
    {
        var cp1 = one.EveryCellPossibilities();
        var pos1 = one.EveryPossibilities();
        var cp2 = two.EveryCellPossibilities();
        var pos2 = two.EveryPossibilities();

        if (cp1.Length == 1 && cp2.Length == 1 && pos1.Count == 1 && pos2.Count == 1 && cp1[0].Cell == cp2[0].Cell)
        {
            foreach (var possibility in solverData.PossibilitiesAt(cp1[0].Cell).EnumeratePossibilities())
            {
                if (pos1.Contains(possibility) || pos2.Contains(possibility)) continue;

                var cp = new CellPossibility(cp1[0].Cell.Row, cp1[0].Cell.Column, possibility);
                if (!nope.Contains(cp)) solverData.ChangeBuffer.ProposePossibilityRemoval(cp);
            }

            return;
        }

        var and = pos1 & pos2;

        foreach (var possibility in and.EnumeratePossibilities())
        {
            List<Cell> cells = new();
            
            foreach (var cp in cp1)
            {
                if (cp.Possibilities.Contains(possibility)) cells.Add(cp.Cell);
            }
            
            foreach (var cp in cp2)
            {
                if (cp.Possibilities.Contains(possibility)) cells.Add(cp.Cell);
            }

            foreach (var cell in SudokuUtility.SharedSeenCells(cells))
            {
                var cp = new CellPossibility(cell.Row, cell.Column, possibility);
                if (!nope.Contains(cp)) solverData.ChangeBuffer.ProposePossibilityRemoval(cp);
            }
        }
    }
}

public class BlossomLoopReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly Loop<ISudokuElement, LinkStrength> _loop;
    private readonly BlossomLoopBranch[] _branches;
    private readonly CellPossibility[] _cps;

    public BlossomLoopReportBuilder(Loop<ISudokuElement, LinkStrength> loop, BlossomLoopBranch[] branches, CellPossibility[] cps)
    {
        _loop = loop;
        _branches = branches;
        _cps = cps;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        var branchesHighlight = new Highlight<ISudokuHighlighter>[_branches.Length];

        for (int i = 0; i < _branches.Length; i++)
        {
            var iForDelegate = i;
            branchesHighlight[i] = lighter =>
            {
                var current = _branches[iForDelegate];
                for (int j = 0; j < current.Branch.Links.Count; j++)
                {
                    lighter.CreateLink(current.Branch.Elements[j], current.Branch.Elements[j + 1], current.Branch.Links[j]);
                    var color = current.Branch.Links[j] == LinkStrength.Weak
                        ? StepColor.On
                        : StepColor.Cause1;
                    lighter.HighlightElement(current.Branch.Elements[j], color);
                    if (j == current.Branch.Links.Count - 1)
                    {
                        lighter.HighlightElement(current.Branch.Elements[j + 1], color == StepColor.On ?
                            StepColor.Cause1 : StepColor.On);
                    }
                    
                    foreach (var cp in _cps)
                    {
                        lighter.EncirclePossibility(cp);
                    }
                
                    ChangeReportHelper.HighlightChanges(lighter, changes);
                }
            };
        }
        
        return new ChangeReport<ISudokuHighlighter>(Explanation(), lighter =>
        {
            var coloring = _loop.Links[0] == LinkStrength.Strong
                ? StepColor.Cause1
                : StepColor.On;
                
            foreach (var element in _loop)
            {
                lighter.HighlightElement(element, coloring);
                coloring = coloring == StepColor.On
                    ? StepColor.Cause1
                    : StepColor.On;
            }

            for (int i = 0; i < _loop.Links.Count; i++)
            {
                lighter.CreateLink(_loop.Elements[i], _loop.Elements[i < _loop.Elements.Count - 1 ? i + 1 : 0],
                    _loop.Links[i]);
            }

            foreach (var cp in _cps)
            {
                lighter.EncirclePossibility(cp);
            }
                
            ChangeReportHelper.HighlightChanges(lighter, changes);
        }, branchesHighlight);
    }

    private string Explanation()
    {
        return $"Loop : {_loop}\nBranches :\n{_branches.ToStringSequence("\n")}";
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}