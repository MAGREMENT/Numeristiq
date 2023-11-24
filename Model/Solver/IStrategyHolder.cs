namespace Model.Solver;

public interface IStrategyHolder
{
    public void ClearStrategies();
    public void AddStrategy(IStrategy strategy);
    public void ClearExcluded();
    public void ExcludeStrategy(int number);
}