namespace Model.Sudoku.Solver.Strategies.UniquenessClueCover;

public interface IPatternCollection
{
    public IStrategy? Strategy { set; }

    public bool Filter(IStrategyUser strategyUser);
    public bool Apply(IStrategyUser strategyUser);
}