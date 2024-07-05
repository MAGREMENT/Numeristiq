using System.Collections.Generic;
using Model.Core.Highlighting;
using Model.Sudokus;
using Model.Utility;
using Model.Utility.Collections;

namespace Model.Core.Changes;

public static class ChangeReportHelper
{
    public static void HighlightChanges(INumericSolvingStateHighlighter highlightable, IReadOnlyList<NumericChange> changes)
    {
        foreach (var change in changes)
        {
            HighlightChange(highlightable, change);
        }
    }
    
    public static void HighlightChange(INumericSolvingStateHighlighter highlightable, NumericChange progress)
    {
        if(progress.Type == ChangeType.PossibilityRemoval)
            highlightable.HighlightPossibility(progress.Number, progress.Row, progress.Column, ChangeColoration.ChangeTwo);
        else highlightable.HighlightCell(progress.Row, progress.Column, ChangeColoration.ChangeOne);
    }
    
    public static string ToName(int n)
    {
        return n switch
        {
            2 => "Doubles",
            3 => "Triples",
            4 => "Quads",
            _ => string.Empty
        };
    }

    public static string XSetStrategyDescription(IReadOnlyList<Cell> cells, string type, int count,
        IEnumerable<int> poss)
    {
        return $"{type} {ToName(count)} in {cells.ToStringSequence(", ")} for " + 
               poss.ToStringSequence(", ");
    }
}