using Global;

namespace Model.Solver.Explanation;

public class CellExplanationElement
{
    private readonly Cell _cell;

    public Cell Value => _cell;

    public CellExplanationElement(Cell cell)
    {
        _cell = cell;
    }

    public override string ToString()
    {
        return _cell.ToString();
    }
}