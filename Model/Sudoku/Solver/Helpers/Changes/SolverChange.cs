namespace Model.Sudoku.Solver.Helpers.Changes;

public readonly struct SolverChange
{
    private readonly int _bits;
    
    public SolverChange(ChangeType changeType, int number, int row, int column)
    {
        _bits = column & 0x1F | ((row & 0x1F) << 5) | ((number & 0x1F) << 10) | ((int)changeType << 15);
    }

    public ChangeType ChangeType => (ChangeType) (_bits >> 15);
    public int Number => (_bits >> 10) & 0x1F;
    public int Row => (_bits >> 5) & 0x1F;
    public int Column => _bits & 0x1F;

    public override string ToString()
    {
        string action = ChangeType == ChangeType.Solution ? "==" : "<>";
        return $"[{Row + 1}, {Column + 1}] {action} {Number}";
    }
}

public enum ChangeType
{
    Possibility = 0, Solution = 1
}