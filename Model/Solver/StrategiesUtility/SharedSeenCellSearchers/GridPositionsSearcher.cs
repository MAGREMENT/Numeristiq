using System.Collections.Generic;
using Global;
using Model.Solver.Position;

namespace Model.Solver.StrategiesUtility.SharedSeenCellSearchers;

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

    public IEnumerable<Cell> SharedSeenEmptyCells(IStrategyManager strategyManager, int row1, int col1, int row2, int col2)
    {
        return ISharedSeenCellSearcher.DefaultSharedSeenEmptyCells(this, strategyManager, row1, col1, row2, col2);
    }

    public IEnumerable<CellPossibility> SharedSeenPossibilities(int row1, int col1, int pos1, int row2, int col2, int pos2)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<CellPossibility> SharedSeenExistingPossibilities(IStrategyManager strategyManager, int row1, int col1, int pos1, int row2,
        int col2, int pos2)
    {
        throw new System.NotImplementedException();
    }
}