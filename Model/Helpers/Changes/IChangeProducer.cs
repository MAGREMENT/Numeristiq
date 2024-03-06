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

public interface ILogManagedChangeProducer : IChangeProducer
{
    public LogManager LogManager { get; }
    public IPossibilitiesHolder TakeSnapshot(); //TODO use current state for this
    public IUpdatableSolvingState CurrentState { get; }
}