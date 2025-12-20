using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Tectonics.Solver;
using Model.Utility;

namespace DesktopApplication.Presenter.Tectonics.Solve;

public class TectonicHighlightTranslator : ITectonicHighlighter, IHighlighterTranslator<ITectonicHighlighter>
{
    private readonly ITectonicDrawer _drawer;

    public TectonicHighlightTranslator(ITectonicDrawer drawer)
    {
        _drawer = drawer;
    }

    public void Translate(IHighlightable<ITectonicHighlighter> highlightable, bool clear)
    {
        if(clear) _drawer.ClearHighlights();
        highlightable.Highlight(this);
        _drawer.Refresh();
    }

    public void HighlightPossibility(int possibility, int row, int col, StepColor color)
    {
        _drawer.FillPossibility(row, col, possibility, color);
    }

    public void HighlightCell(int row, int col, StepColor color)
    {
        _drawer.FillCell(row, col, color);
    }

    public void CreateLink(CellPossibility from, CellPossibility to, LinkStrength linkStrength)
    {
        _drawer.CreateLink(from.Row, from.Column, from.Possibility, to.Row,
            to.Column, to.Possibility, linkStrength, LinkOffsetSidePriority.Any);
    }
}