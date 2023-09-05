namespace Model.Solver;

public interface IStrategyHolder
{
    void SetStrategies(IStrategy[] strategies);
    void SetExcludedStrategies(ulong excluded);
}