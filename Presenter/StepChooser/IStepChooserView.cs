using Presenter.Translators;

namespace Presenter.StepChooser;

public interface IStepChooserView : ISudokuDrawer
{
    void SetCellTo(int row, int col, int number);
    void SetCellTo(int row, int col, int[] possibilities);
    void ShowCommits(ViewCommit[] commits);
    void ShowCommitInformation(ViewCommitInformation commit);
}