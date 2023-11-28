namespace Model.Solver.Strategies;

public class AlmostHiddenSetsChainStrategy : AbstractStrategy
{
    public const string OfficialName = "Almost Hidden Sets Chain";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public AlmostHiddenSetsChainStrategy() : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
    }

    
    public override void Apply(IStrategyManager strategyManager)
    {
        var graph = strategyManager.PreComputer.AlmostHiddenSetGraph();
        
        
    }
}