namespace Model.Solver.Strategies.UniquenessClueCover;

public interface IPatternCollection
{
    public IStrategy? Strategy { set; }

    public bool Filter(IStrategyManager strategyManager);
    public bool Apply(IStrategyManager strategyManager);
}