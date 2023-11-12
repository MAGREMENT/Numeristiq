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

    public bool ContainsAny(int row, int col, Possibilities possibilities)
    {
        var solved = Sudoku[row, col];
        return solved == 0 ? PossibilitiesAt(row, col).PeekAny(possibilities) : possibilities.Peek(solved);
    }

    public bool Contains(int row, int col, int possibility)
    {
        var solved = Sudoku[row, col];
        return solved == 0 ? PossibilitiesAt(row, col).Peek(possibility) : solved == possibility;
    }
    
    IReadOnlyLinePositions ColumnPositionsAt(int col, int number);

    IReadOnlyLinePositions RowPositionsAt(int row, int number);

    IReadOnlyMiniGridPositions MiniGridPositionsAt(int miniRow, int miniCol, int number);

    IReadOnlyGridPositions PositionsFor(int number);
}