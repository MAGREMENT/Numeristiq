using System.Collections.Generic;
using Model.StrategiesUtil;

namespace Model.Solver.StrategiesUtil.SharedCellSearcher;

public class SeenCellCompareSearcher : ISharedSeenCellSearcher
{
    public IEnumerable<Cell> SharedSeenCells(int row1, int col1, int row2, int col2)
    {
        for (int unit = 0; unit < 9; unit++)
        {
            if (unit != row1)
            {
                if (Cells.ShareAUnit(unit, col1, row2, col2) &&
                    !(unit == row2 && col1 == col2)) yield return new Cell(unit, col1);
            }

            if (unit != col1)
            {
                if (Cells.ShareAUnit(row1, unit, row2, col2) &&
                    !(row1 == row2 && unit == col2)) yield return new Cell(row1, unit);
            }
        }

        int startRow = row1 / 3 * 3;
        int startCol = col1 / 3 * 3;
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                int row = startRow + gridRow;
                int col = startCol + gridCol;
                
                if(row == row1 || col == col1 || (row == row2 && col == col2)) continue;

                if (Cells.ShareAUnit(row, col, row2, col2)) yield return new Cell(row, col);
            }
        }
    }

    public IEnumerable<Cell> SharedSeenEmptyCells(IStrategyManager strategyManager, int row1, int col1, int row2, int col2)
    {
        return ISharedSeenCellSearcher.DefaultSharedSeenEmptyCells(this, strategyManager, row1, col1, row2, col2);
    }
}