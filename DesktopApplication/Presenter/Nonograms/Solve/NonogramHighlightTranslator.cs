using System.Collections.Generic;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;

namespace DesktopApplication.Presenter.Nonograms.Solve;

public class NonogramHighlightTranslator : INonogramHighlighter, IHighlighterTranslator<INonogramHighlighter>
{
    private readonly INonogramDrawer _drawer;

    public NonogramHighlightTranslator(INonogramDrawer drawer)
    {
        _drawer = drawer;
    }

    public void Translate(IHighlightable<INonogramHighlighter> highlightable, bool clear)
    {
        if(clear) _drawer.ClearHighlights();
        highlightable.Highlight(this);
        _drawer.Refresh();
    }

    public void HighlightValues(Orientation orientation, int unit, int startIndex, int endIndex, StepColor color)
    {
        _drawer.HighlightValues(unit, startIndex, endIndex, color, orientation);
    }

    public void EncircleCells(IEnumerable<Cell> cells, StepColor color)
    {
        _drawer.EncircleCells(new HashSet<Cell>(cells), color);
    }

    public void EncircleLineSection(Orientation orientation, int unit, int startIndex, int endIndex, StepColor color)
    {
        _drawer.EncircleSection(unit, startIndex, endIndex, color, orientation);
    }
}