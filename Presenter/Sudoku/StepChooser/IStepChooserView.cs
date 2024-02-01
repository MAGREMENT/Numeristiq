using Presenter.Sudoku.Solver;
using Presenter.Sudoku.Translators;

namespace Presenter.Sudoku.StepChooser;

public interface IStepChooserView : ISolverDrawer
{
    
    void ShowCommits(ViewCommit[] commits);
    void ShowCommitInformation(ViewCommitInformation commit);
    void StopShowingCommitInformation();
    void ShowSelection(int n);
    void UnShowSelection(int n);
    void AllowChoosing(bool yes);
}