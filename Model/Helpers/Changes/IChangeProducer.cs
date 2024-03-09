using Model.Helpers.Logs;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.StrategiesUtility;

namespace Model.Helpers.Changes;

public interface IChangeProducer
{
    public bool CanRemovePossibility(CellPossibility cp);
    public bool CanAddSolution(CellPossibility cp);
    public bool ExecuteChange(SolverProgress progress);
}

public interface ILogManagedChangeProducer<out TState> : IChangeProducer where TState : IUpdatableSolvingState
{
    public LogManager LogManager { get; }
    public TState CurrentState { get; }
}