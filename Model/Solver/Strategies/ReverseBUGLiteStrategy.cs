namespace Model.Solver.Strategies;

public class ReverseBUGLiteStrategy : AbstractStrategy
{
    public const string OfficialName = "Reverse BUG-Lite";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public ReverseBUGLiteStrategy() : base(OfficialName, StrategyDifficulty.Medium, DefaultBehavior)
    {
    }
    public override void Apply(IStrategyManager strategyManager)
    {
        for (int u1 = 0; u1 < 8; u1++)
        {
            for (int u2 = u1 + 1; u2 < 8; u2++)
            {
                //Rows
                
            }
        }
    }
}