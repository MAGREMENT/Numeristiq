namespace Model.Sudokus.Solver.Strategies.UniquenessClueCover;

public interface IPatternCollection
{
    public SudokuStrategy? Strategy { set; }

    public bool Filter(ISudokuSolverData solverData);
    public bool Apply(ISudokuSolverData solverData);
}