namespace Model.Sudokus.Solver.Strategies.UniquenessClueCover;

public interface IPatternCollection
{
    public SudokuStrategy? Strategy { set; }

    public bool Filter(ISudokuStrategyUser strategyUser);
    public bool Apply(ISudokuStrategyUser strategyUser);
}