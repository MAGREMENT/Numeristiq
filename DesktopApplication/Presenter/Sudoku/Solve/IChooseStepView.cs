using Model.Helpers.Changes;

namespace DesktopApplication.Presenter.Sudoku.Solve;

public interface IChooseStepView
{
    public ISudokuDrawer Drawer { get; }
    
    public void ClearCommits();
    public void AddCommit(ICommitMaker maker, int index);
    public void SetPreviousPageExistence(bool exists);
    public void SetNextPageExistence(bool exists);
    public void SetTotalPage(int n);
    public void SetCurrentPage(int n);
}