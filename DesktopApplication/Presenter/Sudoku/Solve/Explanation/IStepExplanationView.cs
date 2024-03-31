using Model.Sudoku.Solver.Explanation;

namespace DesktopApplication.Presenter.Sudoku.Solve.Explanation;

public interface IStepExplanationView
{
    public ISudokuSolverDrawer Drawer { get; }
    public void ShowExplanation(ExplanationElement? start);
}