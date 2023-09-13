using Model.Solver.Helpers;

namespace Model.Solver.Strategies;

public class NoStrategy : IStrategy
{
    public string Name => "No strategy";
    
    public StrategyLevel Difficulty => StrategyLevel.None;
    public StatisticsTracker Tracker { get; } = new();

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        
    }
}