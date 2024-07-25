using Model.Core.Explanation;

namespace DesktopApplication.Presenter.Sudokus.Solve.Explanation;

public interface IStepExplanationView
{
    T GetDrawer<T>() where T : IDrawer;
    public IExplanationHighlighter ExplanationHighlighter { get; }
    public void ShowExplanation(ExplanationElement? start);
}