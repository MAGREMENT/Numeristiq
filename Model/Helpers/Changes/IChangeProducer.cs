using Model.Helpers.Highlighting;
using Model.Helpers.Steps;
using Model.Sudokus.Solver.Utility;

namespace Model.Helpers.Changes;

public interface IChangeProducer
{
    public bool CanRemovePossibility(CellPossibility cp);
    public bool CanAddSolution(CellPossibility cp);
    public bool ExecuteChange(SolverProgress progress);
    public void FakeChange();
}

public interface IStepManagingChangeProducer<out TState, THighlighter> : IChangeProducer where TState : IUpdatableSolvingState where THighlighter : ISolvingStateHighlighter
{
    public StepHistory<THighlighter> StepHistory { get; }
    public TState CurrentState { get; }
}