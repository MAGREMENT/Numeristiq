namespace Model;

public interface IStrategy
{
    public string Name { get; }
    public StrategyLevel Difficulty { get; }

    void ApplyOnce(ISolverView solverView);
}

public enum StrategyLevel
{
    None, Basic, Easy, Medium, Hard, Extreme, ByTrial
}