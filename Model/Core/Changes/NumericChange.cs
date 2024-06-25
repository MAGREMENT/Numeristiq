using Model.Utility;

namespace Model.Core.Changes;

public readonly struct NumericChange
{
    private readonly int _bits;
    
    public NumericChange(ChangeType changeType, int number, int row, int column)
    {
        _bits = column & 0x1F | ((row & 0x1F) << 5) | ((number & 0x1F) << 10) | ((int)changeType << 15);
    }

    public NumericChange(ChangeType changeType, CellPossibility cp) : this(changeType, cp.Possibility, cp.Row, cp.Column)
    {
        
    }

    public ChangeType Type => (ChangeType) (_bits >> 15);
    public int Number => (_bits >> 10) & 0x1F;
    public int Row => (_bits >> 5) & 0x1F;
    public int Column => _bits & 0x1F;

    public override string ToString()
    {
        string action = Type == ChangeType.SolutionAddition ? "==" : "<>";
        return $"r{Row + 1}c{Column + 1} {action} {Number}";
    }
}

public readonly struct DichotomousChange
{
    private readonly int _bits;
    
    public DichotomousChange(ChangeType changeType, int row, int column)
    {
        _bits = column & 0x1F | ((row & 0x1F) << 5) | ((int)changeType << 10);
    }

    public DichotomousChange(ChangeType changeType, Cell cell) : this(changeType, cell.Row, cell.Column)
    {
        
    }

    public ChangeType Type => (ChangeType) (_bits >> 10);
    public int Row => (_bits >> 5) & 0x1F;
    public int Column => _bits & 0x1F;

    public override string ToString()
    {
        string action = Type == ChangeType.SolutionAddition ? "O" : "X";
        return $"r{Row + 1}c{Column + 1} == {action}";
    }
}

public enum ChangeType
{
    PossibilityRemoval = 0, SolutionAddition = 1
}