using Model.Solver.Helpers;

namespace Model.Solver;

public abstract class AbstractStrategy : IStrategy //TODO use for every strategy, implement uniqueness
{ 
    public string Name { get; }
    public StrategyDifficulty Difficulty { get; }
    public StatisticsTracker Tracker { get; } = new();

    protected AbstractStrategy(string name, StrategyDifficulty difficulty)
    {
        Name = name;
        Difficulty = difficulty;
    }
    
    public abstract void ApplyOnce(IStrategyManager strategyManager);
}