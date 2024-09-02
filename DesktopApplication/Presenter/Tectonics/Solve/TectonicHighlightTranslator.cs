using System;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Tectonics.Solver.Utility;
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

    public void HighlightElement(ITectonicElement element, StepColor color)
    {
        foreach (var cp in element.EnumerateCellPossibility())
        {
            HighlightPossibility(cp.Possibility, cp.Row, cp.Column, color);
        }
    }

    public void CreateLink(ITectonicElement from, ITectonicElement to, LinkStrength linkStrength)
    {
        if (from is CellPossibility cpF)
        {
            foreach (var cpT in to.EnumerateCellPossibility())
            {
                CreateLink(cpF, cpT, linkStrength);
            }
        }
        else if (from is ZoneGroup zg && to is CellPossibility cpT)
        {
            var min = zg.Cells[0];
            var minDist = Distance(min, cpT.ToCell());

            for (int i = 1; i < zg.Cells.Count; i++)
            {
                var dist = Distance(zg.Cells[i], cpT.ToCell());
                if (dist < minDist)
                {
                    minDist = dist;
                    min = zg.Cells[i];
                }
            }
            
            CreateLink(new CellPossibility(min, zg.Possibility), cpT, linkStrength);
        }
    }
    
    private static double Distance(Cell oneCell, Cell twoCell)
    {
        var oneX = oneCell.Column * 3;
        var oneY = oneCell.Row * 3;

        var twoX = twoCell.Column * 3;
        var twoY = twoCell.Row * 3;

        var dx = twoX - oneX;
        var dy = twoY - oneY;

        return Math.Sqrt(dx * dx + dy * dy);
    }
}