using Model.Positions;
using Model.Possibilities;
using Model.StrategiesUtil;

namespace Model.Solver;

public interface ISolver
{
    IReadOnlyPossibilities PossibilitiesAt(int row, int col);

    IReadOnlyPossibilities PossibilitiesAt(Cell cell)
    {
        return PossibilitiesAt(cell.Row, cell.Col);
    }
    
    LinePositions ColumnPositionsAt(int col, int number);

    LinePositions RowPositionsAt(int row, int number);

    MiniGridPositions MiniGridPositionsAt(int miniRow, int miniCol, int number);
}