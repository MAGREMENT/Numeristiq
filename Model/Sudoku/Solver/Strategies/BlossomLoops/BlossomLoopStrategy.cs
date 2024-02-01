using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Sudoku.Solver.Helpers.Changes;
using Model.Sudoku.Solver.Helpers.Highlighting;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Utility;

namespace Model.Sudoku.Solver.Strategies.BlossomLoops;

public class BlossomLoopStrategy : AbstractStrategy
{
    public const string OfficialNameForCell = "Cell Blossom Loop";
    public const string OfficialNameForUnit = "Unit Blossom Loop";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly IBlossomLoopType _type;
    private readonly IBlossomLoopLoopFinder _loopFinder;
    private readonly IBlossomLoopBranchFinder _branchFinder;
    
    public BlossomLoopStrategy(IBlossomLoopLoopFinder loopFinder, IBlossomLoopBranchFinder branchFinder, IBlossomLoopType type)
        : base("", StrategyDifficulty.Extreme, DefaultBehavior)
    {
        _loopFinder = loopFinder;
        _branchFinder = branchFinder;
        _type = type;
        Name = type.Name;
    }

    
    public override void Apply(IStrategyManager strategyManager)
    {
        strategyManager.GraphManager.ConstructComplex(ConstructRule.PointingPossibilities,
            ConstructRule.CellStrongLink, ConstructRule.CellWeakLink, ConstructRule.UnitStrongLink, ConstructRule.UnitWeakLink);
        var graph = strategyManager.GraphManager.ComplexLinkGraph;

        foreach (var cps in _type.Candidates(strategyManager))
        {
            foreach (var loop in _loopFinder.Find(cps, graph))
            {
                var branches = _branchFinder.FindShortestBranches(graph, cps, loop);
                if (branches is null) continue;

                var nope = SetUpNope(loop, branches);
                
                loop.ForEachLink((one, two) => HandleWeakLoopLink(strategyManager,
                    one, two, nope, branches), LinkStrength.Weak);

                foreach (var b in branches)
                {
                    for (int i = 0; i < b.Branch.Links.Length; i++)
                    {
                        if(b.Branch.Links[i] == LinkStrength.Weak) HandleWeakBranchLink(strategyManager,
                            b.Branch.Elements[i], b.Branch.Elements[i + 1], nope);
                    }
                }

                if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                        new BlossomLoopReportBuilder(loop, branches, cps)) &&
                            OnCommitBehavior == OnCommitBehavior.Return) return;
            }
        }
    }

    private HashSet<CellPossibility> SetUpNope(LinkGraphLoop<ISudokuElement> loop,
        BlossomLoopBranch[] branches)
    {
        HashSet<CellPossibility> nope = new();
        foreach (var b in branches)
        {
            foreach (var element in b.Branch.Elements)
            {
                foreach (var cp in element.EveryCellPossibility()) nope.Add(cp);
            }
        }
        
        foreach (var element in loop)
        {
            foreach (var cp in element.EveryCellPossibility()) nope.Add(cp);
        }

        return nope;
    }

    private void HandleWeakLoopLink(IStrategyManager strategyManager, ISudokuElement one, ISudokuElement two,
        HashSet<CellPossibility> nope, BlossomLoopBranch[] branches)
    {
        List<ISudokuElement> toTakeIntoAccount = new();
        foreach (var b in branches)
        {
            if (one.Equals(b.Targets[0]) && two.Equals(b.Targets[1])) toTakeIntoAccount.Add(b.Branch.Elements[^1]);
        }

        if (toTakeIntoAccount.Count == 0) HandleWeakBranchLink(strategyManager, one, two, nope);
        else
        {
            var and = one.EveryPossibilities().And(two.EveryPossibilities());
            var or = one.EveryPossibilities().Or(two.EveryPossibilities());
            var cells = new HashSet<Cell>(one.EveryCell());
            cells.UnionWith(two.EveryCell());

            foreach (var element in toTakeIntoAccount)
            {
                and = and.And(element.EveryPossibilities());
                or = or.Or(element.EveryPossibilities());
                cells.UnionWith(element.EveryCell());
            }

            if (cells.Count == 1)
            {
                var c = cells.First();
                foreach (var p in strategyManager.PossibilitiesAt(c))
                {
                    if (!or.Peek(p)) strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, c);
                }
            }

            foreach (var p in and)
            {
                List<Cell> c = new();
                
                foreach (var cp in one.EveryCellPossibilities())
                {
                    if(cp.Possibilities.Peek(p)) c.Add(cp.Cell);
                }
                
                foreach (var cp in two.EveryCellPossibilities())
                {
                    if(cp.Possibilities.Peek(p)) c.Add(cp.Cell);
                }

                foreach (var element in toTakeIntoAccount)
                {
                    foreach (var cp in element.EveryCellPossibilities())
                    {
                        if(cp.Possibilities.Peek(p)) c.Add(cp.Cell);
                    }
                }

                foreach (var ssc in Cells.SharedSeenCells(c))
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, ssc);
                }
            }
        }
        
    }

    private void HandleWeakBranchLink(IStrategyManager strategyManager, ISudokuElement one, ISudokuElement two,
        HashSet<CellPossibility> nope)
    {
        var cp1 = one.EveryCellPossibilities();
        var pos1 = one.EveryPossibilities();
        var cp2 = two.EveryCellPossibilities();
        var pos2 = two.EveryPossibilities();

        if (cp1.Length == 1 && cp2.Length == 1 && pos1.Count == 1 && pos2.Count == 1 && cp1[0].Cell == cp2[0].Cell)
        {
            foreach (var possibility in strategyManager.PossibilitiesAt(cp1[0].Cell))
            {
                if (pos1.Peek(possibility) || pos2.Peek(possibility)) continue;

                var cp = new CellPossibility(cp1[0].Cell.Row, cp1[0].Cell.Column, possibility);
                if (!nope.Contains(cp)) strategyManager.ChangeBuffer.ProposePossibilityRemoval(cp);
            }

            return;
        }

        var and = pos1.And(pos2);

        foreach (var possibility in and)
        {
            List<Cell> cells = new();
            
            foreach (var cp in cp1)
            {
                if (cp.Possibilities.Peek(possibility)) cells.Add(cp.Cell);
            }
            
            foreach (var cp in cp2)
            {
                if (cp.Possibilities.Peek(possibility)) cells.Add(cp.Cell);
            }

            foreach (var cell in Cells.SharedSeenCells(cells))
            {
                var cp = new CellPossibility(cell.Row, cell.Column, possibility);
                if (!nope.Contains(cp)) strategyManager.ChangeBuffer.ProposePossibilityRemoval(cp);
            }
        }
    }
}

public class BlossomLoopReportBuilder : IChangeReportBuilder
{
    private readonly LinkGraphLoop<ISudokuElement> _loop;
    private readonly BlossomLoopBranch[] _branches;
    private readonly CellPossibility[] _cps;

    public BlossomLoopReportBuilder(LinkGraphLoop<ISudokuElement> loop, BlossomLoopBranch[] branches, CellPossibility[] cps)
    {
        _loop = loop;
        _branches = branches;
        _cps = cps;
    }

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        var branchesHighlight = new Highlight[_branches.Length];

        for (int i = 0; i < _branches.Length; i++)
        {
            var iForDelegate = i;
            branchesHighlight[i] = lighter =>
            {
                var current = _branches[iForDelegate];
                for (int j = 0; j < current.Branch.Links.Length; j++)
                {
                    lighter.CreateLink(current.Branch.Elements[j], current.Branch.Elements[j + 1], current.Branch.Links[j]);
                    var color = current.Branch.Links[j] == LinkStrength.Weak
                        ? ChangeColoration.CauseOnOne
                        : ChangeColoration.CauseOffOne;
                    lighter.HighlightLinkGraphElement(current.Branch.Elements[j], color);
                    if (j == current.Branch.Links.Length - 1)
                    {
                        lighter.HighlightLinkGraphElement(current.Branch.Elements[j + 1], color == ChangeColoration.CauseOnOne ?
                            ChangeColoration.CauseOffOne : ChangeColoration.CauseOnOne);
                    }
                    
                    foreach (var cp in _cps)
                    {
                        lighter.EncirclePossibility(cp);
                    }
                
                    IChangeReportBuilder.HighlightChanges(lighter, changes);
                }
            };
        }
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(), lighter =>
        {
            var coloring = _loop.Links[0] == LinkStrength.Strong
                ? ChangeColoration.CauseOffOne
                : ChangeColoration.CauseOnOne;
                
            foreach (var element in _loop)
            {
                lighter.HighlightLinkGraphElement(element, coloring);
                coloring = coloring == ChangeColoration.CauseOnOne
                    ? ChangeColoration.CauseOffOne
                    : ChangeColoration.CauseOnOne;
            }

            for (int i = 0; i < _loop.Links.Length; i++)
            {
                lighter.CreateLink(_loop.Elements[i], _loop.Elements[i + 1], _loop.Links[i]);
            }

            foreach (var cp in _cps)
            {
                lighter.EncirclePossibility(cp);
            }
                
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        }, branchesHighlight);
    }

    private string Explanation()
    {
        var builder = new StringBuilder($"Loop : {_loop}\nBranches :\n");
        foreach (var b in _branches)
        {
            builder.Append(b + "\n");
        }

        return builder.ToString();
    }
}