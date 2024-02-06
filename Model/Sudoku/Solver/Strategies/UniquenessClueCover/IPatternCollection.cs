namespace Model.Sudoku.Solver.Strategies.UniquenessClueCover;

public interface IPatternCollection
{
    public ISudokuStrategy? Strategy { set; }

    public bool Filter(IStrategyUser strategyUser);
    public bool Apply(IStrategyUser strategyUser);
}