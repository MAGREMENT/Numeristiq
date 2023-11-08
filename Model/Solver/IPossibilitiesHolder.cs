using Model.Solver.Position;
using Model.Solver.Possibility;
using Model.Solver.StrategiesUtil;

namespace Model.Solver;

public interface IPossibilitiesHolder
{
    IReadOnlySudoku Sudoku { get; }
    
    IReadOnlyPossibilities PossibilitiesAt(int row, int col);

    IReadOnlyPossibilities PossibilitiesAt(Cell cell)
    {
        return PossibilitiesAt(cell.Row, cell.Col);
    }
    
    IReadOnlyLinePositions ColumnPositionsAt(int col, int number);

    IReadOnlyLinePositions RowPositionsAt(int row, int number);

    IReadOnlyMiniGridPositions MiniGridPositionsAt(int miniRow, int miniCol, int number);

    IReadOnlyGridPositions PositionsFor(int number);
}