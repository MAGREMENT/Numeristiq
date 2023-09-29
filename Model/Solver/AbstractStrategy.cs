using Model.Solver.Helpers;

namespace Model.Solver;

public abstract class AbstractStrategy : IStrategy
{ 
    public string Name { get; protected init; }
    public StrategyDifficulty Difficulty { get; protected init; }
    public UniquenessDependency UniquenessDependency { get; protected init; }
    public StatisticsTracker Tracker { get; } = new();

    protected AbstractStrategy(string name, StrategyDifficulty difficulty)
    {
        Name = name;
        Difficulty = difficulty;
        UniquenessDependency = UniquenessDependency.NotDependent;
    }
    
    public abstract void ApplyOnce(IStrategyManager strategyManager);
}