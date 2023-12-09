namespace Model.Solver.Strategies;

public class OddagonStrategy : AbstractStrategy
{
    public const string OfficialName = "Oddagon";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public OddagonStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior)
    {
    }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        throw new System.NotImplementedException();
    }
}