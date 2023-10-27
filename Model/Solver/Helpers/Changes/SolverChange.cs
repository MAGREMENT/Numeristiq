namespace Model.Solver.Helpers.Changes;

public readonly struct SolverChange
{
    public SolverChange(ChangeType changeType, int number, int row, int column)
    {
        ChangeType = changeType;
        Number = number;
        Row = row;
        Column = column;
    }

    public ChangeType ChangeType { get; }
    public int Number { get; }
    public int Row { get; }
    public int Column { get; }

    public override string ToString()
    {
        string action = ChangeType == ChangeType.Solution ? "added as definitive" : "removed from possibilities";
        return $"[{Row + 1}, {Column + 1}] {Number} {action}";
    }
}

public enum ChangeType
{
    Possibility, Solution
}