using Model.Sudokus.Solver.Explanation;

namespace DesktopApplication.Presenter.Sudokus.Solve.Explanation;

public interface IStepExplanationView
{
    public ISudokuSolverDrawer Drawer { get; }
    public IExplanationHighlighter ExplanationHighlighter { get; }
    public void ShowExplanation(ExplanationElement? start);
}