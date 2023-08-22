namespace Model;

public interface IStrategyHolder
{
    void SetStrategies(IStrategy[] strategies);
    void SetExcludedStrategies(ulong excluded);
}