namespace Model;

public interface IStrategyHolder
{
    void SetStrategies(IStrategy[] strategies);
    void SetExcludedStrategies(int excluded);
}