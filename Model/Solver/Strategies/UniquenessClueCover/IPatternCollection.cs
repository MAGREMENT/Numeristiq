namespace Model.Solver.Strategies.UniquenessClueCover;

public interface IPatternCollection
{
    public IStrategy? Strategy { set; }
    
    public bool Apply(IStrategyManager strategyManager);
}