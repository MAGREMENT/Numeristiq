using System.Collections.Generic;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;

namespace DesktopApplication.Presenter.Nonograms.Solve;

public class NonogramHighlightTranslator : INonogramHighlighter
{
    private readonly INonogramDrawer _drawer;

    public NonogramHighlightTranslator(INonogramDrawer drawer)
    {
        _drawer = drawer;
    }

    public void Translate(IHighlightable<INonogramHighlighter> highlightable)
    {
        highlightable.Highlight(this);
        _drawer.Refresh();
    }

    public void HighlightValues(Orientation orientation, int unit, int startIndex, int endIndex, StepColor color)
    {
        if (orientation == Orientation.Horizontal) _drawer.HighlightHorizontalValues(unit, startIndex, endIndex, color);
        else _drawer.HighlightVerticalValues(unit, startIndex, endIndex, color);
    }

    public void EncircleCells(IEnumerable<Cell> cells, StepColor color)
    {
        _drawer.EncircleCells(new HashSet<Cell>(cells), color);
    }

    public void EncircleLineSection(Orientation orientation, int unit, int startIndex, int endIndex, StepColor color)
    {
        if (orientation == Orientation.Horizontal) _drawer.EncircleRowSection(unit, startIndex, endIndex, color);
        else _drawer.EncircleColumnSection(unit, startIndex, endIndex, color);
    }
}