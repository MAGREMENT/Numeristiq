using Model.Solver.StrategiesUtility;

namespace Model.Solver.Explanation;

public class CellPossibilityExplanationElement
{
    private readonly CellPossibility _cellPossibility;

    public CellPossibility Value => _cellPossibility;

    public CellPossibilityExplanationElement(CellPossibility cellPossibility)
    {
        _cellPossibility = cellPossibility;
    }

    public override string ToString()
    {
        return _cellPossibility.ToString();
    }
}