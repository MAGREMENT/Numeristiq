using Model.Solver.Helpers.Changes;
using Presenter.Solver;

namespace Presenter.StepChooser;

public interface IStepChooserCallback
{
    SolverSettings Settings { get; }
    void EnableActionsBack();
    void ApplyCommit(BuiltChangeCommit commit);
}