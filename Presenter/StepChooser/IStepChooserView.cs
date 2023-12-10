using Presenter.Translators;

namespace Presenter.StepChooser;

public interface IStepChooserView : ISudokuDrawer
{
    
    void ShowCommits(ViewCommit[] commits);
    void ShowCommitInformation(ViewCommitInformation commit);
    void StopShowingCommitInformation();
    void AllowChoosing(bool yes);
}