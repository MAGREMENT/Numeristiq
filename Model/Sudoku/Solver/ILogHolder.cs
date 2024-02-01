namespace Model.Sudoku.Solver;

public interface ILogHolder
{
    public SolverState CurrentState { get; }
}