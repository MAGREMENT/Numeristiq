using System.Collections.Generic;
using System.Text;
using Model.Helpers.Highlighting;

namespace Model.Helpers.Changes;

public interface IChangeReportBuilder<in TVerifier> where TVerifier : ISolvingState
{
    public ChangeReport Build(IReadOnlyList<SolverProgress> changes, TVerifier snapshot);
}

public static class ChangeReportHelper
{
    public static void HighlightChanges(ISudokuHighlighter highlightable, IReadOnlyList<SolverProgress> changes) //TODO to static class
    {
        foreach (var change in changes)
        {
            HighlightChange(highlightable, change);
        }
    }
    
    public static void HighlightChange(ISudokuHighlighter highlightable, SolverProgress progress)
    {
        if(progress.ProgressType == ProgressType.PossibilityRemoval)
            highlightable.HighlightPossibility(progress.Number, progress.Row, progress.Column, ChangeColoration.ChangeTwo);
        else highlightable.HighlightCell(progress.Row, progress.Column, ChangeColoration.ChangeOne);
    }

    public static string ChangesToString(IReadOnlyList<SolverProgress> changes)
    {
        if (changes.Count == 0) return "";
        
        var builder = new StringBuilder();
        foreach (var change in changes)
        {
            var action = change.ProgressType == ProgressType.PossibilityRemoval
                ? "<>"
                : "==";
            builder.Append($"r{change.Row + 1}c{change.Column + 1} {action} {change.Number}, ");
        }

        return builder.ToString()[..^2];
    }
}

public enum ChangeColoration
{
    None = 0, Neutral, ChangeOne, ChangeTwo, CauseOffOne, CauseOffTwo, CauseOffThree,
    CauseOffFour, CauseOffFive, CauseOffSix, CauseOffSeven, CauseOffEight, CauseOffNine,
    CauseOffTen, CauseOnOne
}

public static class ChangeColorationUtility
{
    public static bool IsOff(ChangeColoration coloration)
    {
        return (int)coloration is >= 4 and <= 13;
    }
}

