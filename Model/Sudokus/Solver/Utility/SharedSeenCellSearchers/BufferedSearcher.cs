using System.Collections.Generic;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.SharedSeenCellSearchers;

public class BufferedSearcher : ISharedSeenCellSearcher
{
    private readonly IEnumerable<Cell>[,] _buffer = new IEnumerable<Cell>[80, 81]; //TODO improve

    public BufferedSearcher(ISharedSeenCellSearcher impl)
    {
        for (int first = 0; first < 80; first++)
        {
            for (int second = first + 1; second < 81; second++)
            {
                _buffer[first, second] = impl.SharedSeenCells(first / 9, first % 9, second / 9, second % 9);
            }
        }
    }

    public IEnumerable<Cell> SharedSeenCells(int row1, int col1, int row2, int col2)
    {
        if (row1 == row2 && col1 == col2) return SudokuCellUtility.SeenCells(new Cell(row1, col1));

        var val1 = row1 * 9 + col1;
        var val2 = row2 * 9 + col2;

        if (val2 < val1)
        {
            (val1, val2) = (val2, val1);
        }

        return _buffer[val1, val2];
    }

    public IEnumerable<Cell> SharedSeenEmptyCells(ISudokuSolverData solverData, int row1, int col1, int row2, int col2)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<CellPossibility> SharedSeenPossibilities(int row1, int col1, int pos1, int row2, int col2, int pos2)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<CellPossibility> SharedSeenExistingPossibilities(ISudokuSolverData solverData, int row1, int col1, int pos1, int row2,
        int col2, int pos2)
    {
        throw new System.NotImplementedException();
    }
}