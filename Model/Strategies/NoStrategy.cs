namespace Model.Strategies;

public class NoStrategy : IStrategy
{
    public string Name { get; } = "No strategy";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.None;

    public void ApplyOnce(ISolverView solverView)
    {
        
    }
}