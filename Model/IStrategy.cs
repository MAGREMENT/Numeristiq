namespace Model;

public interface IStrategy
{
    public string Name { get; }
    public StrategyLevel Difficulty { get; }
    
    public int Score { get; set; }

    void ApplyOnce(ISolverView solverView);
}

public enum StrategyLevel
{
    None, Basic, Easy, Medium, Hard, Extreme, ByTrial
}