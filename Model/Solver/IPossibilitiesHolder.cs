using Model.Possibilities;
using Model.StrategiesUtil;

namespace Model.Solver;

public interface IPossibilitiesHolder
{
    IReadOnlyPossibilities PossibilitiesAt(int row, int col);

    IReadOnlyPossibilities PossibilitiesAt(Cell cell)
    {
        return PossibilitiesAt(cell.Row, cell.Col);
    }
}