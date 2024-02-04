using Model.Sudoku.Solver.BitSets;
using Model.Sudoku.Solver.Position;
using Model.Sudoku.Solver.Possibility;
using Model.Utility;

namespace Model.Sudoku.Solver;

public interface IPossibilitiesHolder
{
    IReadOnlySudoku Sudoku { get; }
    
    ReadOnlyBitSet16 PossibilitiesAt(int row, int col);

    ReadOnlyBitSet16 PossibilitiesAt(Cell cell)
    {
        return PossibilitiesAt(cell.Row, cell.Column);
    }

    public bool ContainsAny(int row, int col, ReadOnlyBitSet16 possibilities)
    {
        var solved = Sudoku[row, col];
        return solved == 0 ? PossibilitiesAt(row, col).ContainsAny(possibilities) : possibilities.Contains(solved);
    }

    public bool ContainsAny(Cell cell, ReadOnlyBitSet16 possibilities)
    {
        return ContainsAny(cell.Row, cell.Column, possibilities);
    }

    public bool Contains(int row, int col, int possibility)
    {
        var solved = Sudoku[row, col];
        return solved == 0 ? PossibilitiesAt(row, col).Contains(possibility) : solved == possibility;
    }
    
    IReadOnlyLinePositions ColumnPositionsAt(int col, int number);

    IReadOnlyLinePositions RowPositionsAt(int row, int number);

    IReadOnlyMiniGridPositions MiniGridPositionsAt(int miniRow, int miniCol, int number);

    IReadOnlyGridPositions PositionsFor(int number);
}