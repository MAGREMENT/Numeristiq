using System.Collections.Generic;
using Model.Sudokus.Solver.Position;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.SharedSeenCellSearchers;

public class GridPositionsSearcher : ISharedSeenCellSearcher
{
    public IEnumerable<Cell> SharedSeenCells(int row1, int col1, int row2, int col2)
    {
        var one = new GridPositions();
        var two = new GridPositions();

        one.FillRow(row1);
        one.FillColumn(col1);
        one.FillMiniGrid(row1 / 3, col1 / 3);

        two.FillRow(row2);
        two.FillColumn(col2);
        two.FillMiniGrid(row2 / 3, col2 / 3);

        var and = one.And(two);
        and.Remove(row1, col1);
        and.Remove(row2, col2);

        return and;
    }

    public IEnumerable<Cell> SharedSeenEmptyCells(ISudokuStrategyUser strategyUser, int row1, int col1, int row2, int col2)
    {
        return ISharedSeenCellSearcher.DefaultSharedSeenEmptyCells(this, strategyUser, row1, col1, row2, col2);
    }

    public IEnumerable<CellPossibility> SharedSeenPossibilities(int row1, int col1, int pos1, int row2, int col2, int pos2)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<CellPossibility> SharedSeenExistingPossibilities(ISudokuStrategyUser strategyUser, int row1, int col1, int pos1, int row2,
        int col2, int pos2)
    {
        throw new System.NotImplementedException();
    }
}