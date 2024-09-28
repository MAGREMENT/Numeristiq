using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace DesktopApplication.Presenter.Sudokus.Solve.ChooseStep;

public interface IStepChooserView
{
    public ISudokuSolverDrawer Drawer { get; }
    
    public void ClearSteps();
    public void AddStep(int index, BuiltChangeCommit<NumericChange, ISudokuHighlighter> commit);
    public void SetTotalPage(int n);
    public void SetCurrentPage(int n);
    public void OpenStep(int index);
    public void CloseStep(int index);
    public void SetSelectionAvailability(bool yes);
}