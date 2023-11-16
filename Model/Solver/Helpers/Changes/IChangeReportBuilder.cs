using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

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
        var s = "";
        foreach (var change in changes)
        {
            var action = change.ChangeType == ChangeType.Possibility
                ? "removed from the possibilities"
                : "added as definitive";
            s += $"[{change.Row + 1}, {change.Column + 1}] {change.Number} {action}\n";
        }

        return s;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot);
}