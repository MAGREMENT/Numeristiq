using Presenter.Solver;
using Presenter.Translators;

namespace Presenter.StepChooser;

public interface IStepChooserView : ISolverDrawer
{
    
    void ShowCommits(ViewCommit[] commits);
    void ShowCommitInformation(ViewCommitInformation commit);
    void StopShowingCommitInformation();
    void AllowChoosing(bool yes);
}