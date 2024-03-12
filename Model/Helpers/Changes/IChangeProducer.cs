using Model.Helpers.Highlighting;
using Model.Helpers.Logs;
using Model.Sudoku.Solver.StrategiesUtility;

namespace Model.Helpers.Changes;

public interface IChangeProducer
{
    public bool CanRemovePossibility(CellPossibility cp);
    public bool CanAddSolution(CellPossibility cp);
    public bool ExecuteChange(SolverProgress progress);
}

public interface ILogManagedChangeProducer<out TState, THighlighter> : IChangeProducer where TState : IUpdatableSolvingState where THighlighter : ISolvingStateHighlighter
{
    public LogManager<THighlighter> LogManager { get; }
    public TState CurrentState { get; }
}