namespace Model.Strategies;

public class NoStrategy : IStrategy
{
    public string Name { get; } = "No strategy";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.None;
    public int Score { get; set; }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        
    }
}