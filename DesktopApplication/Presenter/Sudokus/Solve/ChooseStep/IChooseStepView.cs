using Model.Helpers.Changes;

namespace DesktopApplication.Presenter.Sudokus.Solve.ChooseStep;

public interface IChooseStepView
{
    public ISudokuSolverDrawer Drawer { get; }
    
    public void ClearCommits();
    public void AddCommit(ICommitMaker maker, int index);
    public void SetPreviousPageExistence(bool exists);
    public void SetNextPageExistence(bool exists);
    public void SetTotalPage(int n);
    public void SetCurrentPage(int n);
    public void SelectStep(int index);
    public void UnselectStep(int index);
    public void EnableSelection(bool isEnabled);
}