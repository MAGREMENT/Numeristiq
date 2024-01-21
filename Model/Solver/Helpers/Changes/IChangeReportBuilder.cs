using System.Collections.Generic;
using System.Text;
using Global.Enums;
using Model.Solver.Helpers.Highlighting;

namespace Model.Solver.Helpers.Changes;

public interface IChangeReportBuilder
{
    public static void HighlightChanges(IHighlightable highlightable, List<SolverChange> changes)
    {
        foreach (var change in changes)
        {
            HighlightChange(highlightable, change);
        }
    }
    
    public static void HighlightChange(IHighlightable highlightable, SolverChange change)
    {
        if(change.ChangeType == ChangeType.Possibility)
            highlightable.HighlightPossibility(change.Number, change.Row, change.Column, ChangeColoration.ChangeTwo);
        else highlightable.HighlightCell(change.Row, change.Column, ChangeColoration.ChangeOne);
    }

    public static string ChangesToString(List<SolverChange> changes)
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
    
    public static string ChangesToString(IEnumerable<SolverChange> changes)
    {
        var builder = new StringBuilder();
        foreach (var change in changes)
        {
            var action = change.ChangeType == ChangeType.Possibility
                ? "<>"
                : "==";
            builder.Append($"r{change.Row + 1}c{change.Column + 1} {action} {change.Number}\n");
        }

        return builder.ToString();
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot);
}