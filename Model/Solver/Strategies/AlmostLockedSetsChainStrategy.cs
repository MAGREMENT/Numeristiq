using Model.Solver.Position;
using Model.Solver.Possibility;
using Model.Solver.PossibilityPosition;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies;

public class AlmostLockedSetsChainStrategy : AbstractStrategy
{
    public const string OfficialName = "Almost Locked Sets Chain";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    public AlmostLockedSetsChainStrategy() : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
    }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        var graph = strategyManager.PreComputer.AlmostLockedSetGraph();

        foreach (var start in graph)
        {
            
        }
    }

    private bool Search(IStrategyManager strategyManager, PossibilitiesGraph<IPossibilitiesPositions> graph,
        GridPositions explored)
    {
        return false;
    }
}