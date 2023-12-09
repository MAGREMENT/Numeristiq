namespace Model.Solver.Strategies.ForcingNets;

public class OddagonForcingNetStrategy : AbstractStrategy
{
    public const string OfficialName = "Oddagon Forcing Net";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public OddagonForcingNetStrategy() : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
    }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        throw new System.NotImplementedException();
    }
}