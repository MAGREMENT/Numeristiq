namespace Model.SudokuSolving.Solver;

public interface ILogHolder
{
    public SolverState CurrentState { get; }
}