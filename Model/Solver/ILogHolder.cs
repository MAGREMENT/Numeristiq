namespace Model.Solver;

public interface ILogHolder
{
    public string State { get; }
    public int StrategyCount { get; }
}