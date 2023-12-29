namespace Model.Solver.Strategies;

public class NonColorablePatternStrategy : AbstractStrategy
{
    private const string OfficialName = "Non-Colorable Pattern";
    
    public override OnCommitBehavior DefaultOnCommitBehavior { get; }
    
    public NonColorablePatternStrategy(string name, StrategyDifficulty difficulty, OnCommitBehavior defaultBehavior) : base(name, difficulty, defaultBehavior)
    {
    }

    
    public override void Apply(IStrategyManager strategyManager)
    {
        throw new System.NotImplementedException();
    }
}