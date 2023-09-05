namespace Model.Changes;

public class SolverChange
{
    public SolverChange(SolverNumberType numberType, int number, int row, int column)
    {
        NumberType = numberType;
        Number = number;
        Row = row;
        Column = column;
    }

    public SolverNumberType NumberType { get; }
    public int Number { get; }
    public int Row { get; }
    public int Column { get; }

    public override string ToString()
    {
        string action = NumberType == SolverNumberType.Definitive ? "added as definitive" : "removed from possibilities";
        return $"[{Row + 1}, {Column + 1}] {Number} {action}";
    }
}

public enum SolverNumberType
{
    Possibility, Definitive
}