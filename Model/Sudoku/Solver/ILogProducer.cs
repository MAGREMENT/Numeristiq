namespace Model.Sudoku.Solver;

public interface ILogProducer
{
    public SolverState CurrentState { get; }
}