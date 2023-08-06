using System.Collections.Generic;
using System.Printing;
using System.Windows.Documents;
using Model.Logs;

namespace Model;

public interface IChangeReportWaiter
{
    public static void HighLightChanges(IHighLighter highLighter, List<SolverChange> changes)
    {
        foreach (var change in changes)
        {
            if(change.NumberType == SolverNumberType.Possibility)
                highLighter.HighLightPossibility(change.Number, change.Row, change.Column, ChangeColoration.Change);
            else highLighter.HighLightCell(change.Row, change.Column, ChangeColoration.Change);
        }
    }

    public static string ChangesToString(List<SolverChange> changes)
    {
        var s = "";
        foreach (var change in changes)
        {
            var action = change.NumberType == SolverNumberType.Possibility
                ? "removed from the possibilities"
                : "added as definitive";
            s += $"[{change.Row + 1}, {change.Column + 1}] {change.Number} {action}\n";
        }

        return s;
    }

    public ChangeReport Process(List<SolverChange> changes, IChangeManager manager);
}

public interface IHighLighter
{
    public void HighLightPossibility(int possibility, int row, int col, ChangeColoration coloration);

    public void HighLightCell(int row, int col, ChangeColoration coloration);
}

public enum ChangeColoration
{
    Change, CauseOffOne, CauseOffTwo, CauseOffThree, CauseOffFour, CauseOffFive, CauseOffSix, CauseOnOne, Neutral
}

public delegate void HighLightSolver(IHighLighter h);