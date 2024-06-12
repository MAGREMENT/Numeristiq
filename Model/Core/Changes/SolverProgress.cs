using Model.Utility;

namespace Model.Core.Changes;

public readonly struct SolverProgress
{
    private readonly int _bits;
    
    public SolverProgress(ProgressType progressType, int number, int row, int column)
    {
        _bits = column & 0x1F | ((row & 0x1F) << 5) | ((number & 0x1F) << 10) | ((int)progressType << 15);
    }

    public SolverProgress(ProgressType progressType, CellPossibility cp) : this(progressType, cp.Possibility, cp.Row, cp.Column)
    {
        
    }

    public ProgressType ProgressType => (ProgressType) (_bits >> 15);
    public int Number => (_bits >> 10) & 0x1F;
    public int Row => (_bits >> 5) & 0x1F;
    public int Column => _bits & 0x1F;

    public override string ToString()
    {
        string action = ProgressType == ProgressType.SolutionAddition ? "==" : "<>";
        return $"[{Row + 1}, {Column + 1}] {action} {Number}";
    }
}

public enum ProgressType
{
    PossibilityRemoval = 0, SolutionAddition = 1
}