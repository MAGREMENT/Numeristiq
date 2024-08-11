using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace DesktopApplication.Presenter.Kakuros.Solve;

public class KakuroHighlightTranslator : IHighlighterTranslator<INumericSolvingStateHighlighter>,
    INumericSolvingStateHighlighter
{
    private readonly IKakuroSolverDrawer _drawer;

    public KakuroHighlightTranslator(IKakuroSolverDrawer drawer)
    {
        _drawer = drawer;
    }

    public void Translate(IHighlightable<INumericSolvingStateHighlighter> highlightable, bool clear)
    {
        if(clear) _drawer.ClearHighlights();
        highlightable.Highlight(this);
        _drawer.Refresh();
    }

    public void HighlightPossibility(int possibility, int row, int col, StepColor color)
    {
        
    }

    public void HighlightCell(int row, int col, StepColor color)
    {
        
    }

    public void CreateLink(CellPossibility from, CellPossibility to, LinkStrength linkStrength)
    {
        
    }
}