using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Tectonics.Solver.Utility;
using Model.Utility;

namespace Model.Tectonics.Solver;

public interface ITectonicHighlighter : INumericSolvingStateHighlighter { }

public static class ITectonicHighlighterExtensions
{
    public static void HighlightElement(this ITectonicHighlighter h, ITectonicElement element, StepColor color)
    {
        foreach (var cp in element.EnumerateCellPossibility())
        {
            h.HighlightPossibility(cp.Possibility, cp.Row, cp.Column, color);
        }
    }

    public static void CreateLink(this ITectonicHighlighter h, ITectonicElement from, ITectonicElement to, LinkStrength linkStrength)
    {
        if (from is CellPossibility cpF)
        {
            foreach (var cpT in to.EnumerateCellPossibility())
            {
                h.CreateLink(cpF, cpT, linkStrength);
            }
        }
        else if (from is ZoneGroup zg && to is CellPossibility cpT)
        {
            var min = zg.Cells[0];
            var minDist = CellUtility.Distance(min, cpT.ToCell());

            for (int i = 1; i < zg.Cells.Count; i++)
            {
                var dist = CellUtility.Distance(zg.Cells[i], cpT.ToCell());
                if (dist < minDist)
                {
                    minDist = dist;
                    min = zg.Cells[i];
                }
            }
            
            h.CreateLink(new CellPossibility(min, zg.Possibility), cpT, linkStrength);
        }
    }
}