using Model;
using Model.Core;

namespace DesktopApplication.Presenter.Sudokus.Solve.ChooseStep;

public interface IChooseStepView
{
    public ISudokuSolverDrawer Drawer { get; }
    
    public void ClearCommits();
    public void AddCommit(Strategy maker, int index);
    public void SetTotalPage(int n);
    public void SetCurrentPage(int n);
    public void SelectStep(int index);
    public void UnselectStep(int index);
    public void EnableSelection(bool isEnabled);
}