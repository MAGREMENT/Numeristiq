using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;
using Model.Utility.Collections;

namespace DesktopApplication.Presenter.Binairos.Solve;

public class BinairoHighlightTranslator : IHighlighterTranslator<IBinairoHighlighter>, IBinairoHighlighter
{
    private readonly IBinairoDrawer _drawer;

    public BinairoHighlightTranslator(IBinairoDrawer drawer)
    {
        _drawer = drawer;
    }

    public void Translate(IHighlightable<IBinairoHighlighter> highlightable, bool clear)
    {
        if(clear) _drawer.ClearHighlights();
        highlightable.Highlight(this);
        _drawer.Refresh();
    }

    public void HighlightCell(int row, int col, StepColor color)
    {
        _drawer.HighlightCell(row, col, color);
    }

    public void EncircleCells(IContainingEnumerable<Cell> cells, StepColor color)
    {
        _drawer.EncircleCells(cells, color);
    }
}