namespace Model.Solver.Strategies;

public class ExtendedUniqueRectanglesStrategy : AbstractStrategy
{
    public const string OfficialName = "Extended Unique Rectangles";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public ExtendedUniqueRectanglesStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior)
    {
    }

    
    public override void Apply(IStrategyManager strategyManager)
    {
        for (int mini = 0; mini < 3; mini++)
        {
            //TODO
        }
    }
}