using Model.Core.Explanation;

namespace DesktopApplication.Presenter.Sudokus.Solve.Explanation;

public interface IStepExplanationView<out TDrawer>
{
    public TDrawer Drawer { get; }
    public IExplanationHighlighter ExplanationHighlighter { get; }
    public void ShowExplanation(ExplanationElement? start);
}