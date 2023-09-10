using System.Collections.Generic;
using Model.Solver;

namespace Model.StrategiesUtil.SharedCellSearcher;

public class InCommonFindSearcher : ISharedSeenCellSearcher
{
    public IEnumerable<Coordinate> SharedSeenCells(int row1, int col1, int row2, int col2)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<Coordinate> SharedSeenEmptyCells(IStrategyManager strategyManager, int row1, int col1, int row2, int col2)
    {
        return ISharedSeenCellSearcher.DefaultSharedSeenEmptyCells(this, strategyManager, row1, col1, row2, col2);
    }
}