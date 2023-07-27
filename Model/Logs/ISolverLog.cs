using System.Collections.Generic;

namespace Model.Logs;

public interface ISolverLog
{
    public string Title { get; }
    public Intensity Intensity { get; }
    public string Text { get; }
    public string SolverState { get; }

    public void DefinitiveAdded(int n, int row, int col);
    public void PossibilityRemoved(int p, int row, int col);

    public IEnumerable<LogPart> AllParts();
}

public enum Intensity
{
    Zero, One, Two, Three, Four, Five, Six
}

public enum Action
{
    PossibilityRemoved, NumberAdded
}

public class LogPart
{
    public LogPart(Action action, int number, int row, int column)
    {
        Action = action;
        Number = number;
        Row = row;
        Column = column;
    }

    public Action Action { get; }
    public int Number { get; }
    public int Row { get; }
    public int Column { get; }
}