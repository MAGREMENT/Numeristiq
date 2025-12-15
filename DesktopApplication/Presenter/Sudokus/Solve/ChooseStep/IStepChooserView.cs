namespace DesktopApplication.Presenter.Sudokus.Solve.ChooseStep;

public interface IStepChooserView : IStepManagingView
{
    public ISudokuSolverDrawer Drawer { get; }
    
    public void SetTotalPage(int n);
    public void SetCurrentPage(int n);
    public void SetSelectionAvailability(bool yes);
}