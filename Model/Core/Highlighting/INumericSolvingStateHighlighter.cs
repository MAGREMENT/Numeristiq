using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Tectonics.Solver.Utility;
using Model.Utility;

namespace Model.Core.Highlighting;

public interface INumericSolvingStateHighlighter
{
    public void HighlightPossibility(int possibility, int row, int col, StepColor color);
    
    public void HighlightCell(int row, int col, StepColor color);
    
    public void CreateLink(CellPossibility from, CellPossibility to, LinkStrength linkStrength);
}

public static class INumericSolvingStateHighlighterExtensions
{
    public static void HighlightPossibility(this INumericSolvingStateHighlighter h, CellPossibility coord, StepColor color)
    {
        h.HighlightPossibility(coord.Possibility, coord.Row, coord.Column, color);
    }

    public static void HighlightPossibility(this INumericSolvingStateHighlighter h, Cell cell, int possibility, StepColor color)
    {
        h.HighlightPossibility(possibility, cell.Row, cell.Column, color);
    }
    
    public static void HighlightCell(this INumericSolvingStateHighlighter h, Cell coord, StepColor color)
    {
        h.HighlightCell(coord.Row, coord.Column, color);
    }
}