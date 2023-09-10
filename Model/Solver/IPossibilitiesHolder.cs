using Model.Positions;
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
    
    LinePositions ColumnPositions(int col, int number);

    LinePositions RowPositions(int row, int number);

    MiniGridPositions MiniGridPositions(int miniRow, int miniCol, int number);
}