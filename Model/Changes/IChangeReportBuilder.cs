using System.Collections.Generic;
using Model.Solver;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LinkGraph;

namespace Model.Changes;

public interface IChangeReportBuilder
{
    public static void HighlightChanges(IHighlightable highlightable, List<SolverChange> changes)
    {
        foreach (var change in changes)
        {
            if(change.NumberType == SolverNumberType.Possibility)
                highlightable.HighlightPossibility(change.Number, change.Row, change.Column, ChangeColoration.ChangeTwo);
            else highlightable.HighlightCell(change.Row, change.Column, ChangeColoration.ChangeOne);
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

    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager);
}

public interface IHighlightable
{
    public void HighlightPossibility(int possibility, int row, int col, ChangeColoration coloration);

    public void HighlightPossibility(CellPossibility coord, ChangeColoration coloration)
    {
        HighlightPossibility(coord.Possibility, coord.Row, coord.Col, coloration);
    }

    public void CirclePossibility(int possibility, int row, int col);

    public void CirclePossibility(CellPossibility coord)
    {
        CirclePossibility(coord.Possibility, coord.Row, coord.Col);
    }

    public void HighlightCell(int row, int col, ChangeColoration coloration);

    public void HighlightCell(Cell coord, ChangeColoration coloration)
    {
        HighlightCell(coord.Row, coord.Col, coloration);
    }

    public void CircleCell(int row, int col);

    public void CircleCell(Cell coord)
    {
        CircleCell(coord.Row, coord.Col);
    }

    public void HighlightLinkGraphElement(ILinkGraphElement element, ChangeColoration coloration);

    public void CreateLink(CellPossibility from, CellPossibility to, LinkStrength linkStrength);

    public void CreateLink(ILinkGraphElement from, ILinkGraphElement to, LinkStrength linkStrength);
}

public enum ChangeColoration
{
    ChangeOne, ChangeTwo, CauseOffOne, CauseOffTwo, CauseOffThree, CauseOffFour, CauseOffFive, CauseOffSix, CauseOnOne, Neutral
}

public delegate void HighlightSolver(IHighlightable h);

public class HighlightManager
{
    private readonly HighlightSolver[] _highlights;
    private int _cursor;

    public int Count => _highlights.Length;

    public HighlightManager(HighlightSolver highlight)
    {
        _highlights = new[] { highlight };
    }

    public HighlightManager(params HighlightSolver[] highlights)
    {
        _highlights = highlights;
    }

    public void Apply(IHighlightable highlightable)
    {
        if (Count == 0) return;
        _highlights[_cursor](highlightable);
    }

    public void ShiftLeft()
    {
        _cursor--;
        if (_cursor < 0) _cursor += Count;
    }

    public void ShiftRight()
    {
        _cursor = (_cursor + 1) % Count;
    }
}