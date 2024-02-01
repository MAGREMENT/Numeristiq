using System.Collections.Generic;
using System.Text;
using Model.Sudoku.Solver.Helpers.Highlighting;

namespace Model.Sudoku.Solver.Helpers.Changes;

public interface IChangeReportBuilder
{
    public static void HighlightChanges(IHighlighter highlightable, IReadOnlyList<SolverChange> changes)
    {
        foreach (var change in changes)
        {
            HighlightChange(highlightable, change);
        }
    }
    
    public static void HighlightChange(IHighlighter highlightable, SolverChange change)
    {
        if(change.ChangeType == ChangeType.Possibility)
            highlightable.HighlightPossibility(change.Number, change.Row, change.Column, ChangeColoration.ChangeTwo);
        else highlightable.HighlightCell(change.Row, change.Column, ChangeColoration.ChangeOne);
    }

    public static string ChangesToString(IReadOnlyList<SolverChange> changes)
    {
        if (changes.Count == 0) return "";
        
        var builder = new StringBuilder();
        foreach (var change in changes)
        {
            var action = change.ChangeType == ChangeType.Possibility
                ? "<>"
                : "==";
            builder.Append($"r{change.Row + 1}c{change.Column + 1} {action} {change.Number}, ");
        }

        return builder.ToString()[..^2];
    }

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot);
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

public enum ChangeType
{
    Possibility, Solution
}