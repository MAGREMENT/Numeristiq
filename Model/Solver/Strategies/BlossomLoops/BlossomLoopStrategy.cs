using System.Collections.Generic;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies.BlossomLoops;

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
        strategyManager.GraphManager.ConstructComplex(ConstructRule.PointingPossibilities, ConstructRule.AlmostNakedPossibilities,
            ConstructRule.CellStrongLink, ConstructRule.CellWeakLink, ConstructRule.UnitStrongLink, ConstructRule.UnitWeakLink);
        var graph = strategyManager.GraphManager.ComplexLinkGraph;
        HashSet<ILinkGraphElement> used = new();

        foreach (var cps in _type.Candidates(strategyManager))
        {
            foreach (var loop in _loopFinder.Find(cps, graph))
            { 
                var first = loop.Elements[0];
                var last = loop.Elements[^1];

                foreach (var cp in cps)
                {
                    if (cp.Equals(first) || cp.Equals(last)) continue;
                }
                    
                used.Clear();
            }
        }
    }
}