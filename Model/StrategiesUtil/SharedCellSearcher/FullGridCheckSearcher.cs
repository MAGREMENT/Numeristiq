using System.Collections.Generic;
using Model.Solver;

namespace Model.StrategiesUtil.SharedCellSearcher;

public class FullGridCheckSearcher : ISharedSeenCellSearcher
{
    public IEnumerable<Coordinate> SharedSeenCells(int row1, int col1, int row2, int col2)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if ((row == row1 && col == col1) || (row == row2 && col == col2)) continue;
                
                if (CoordinateUtils.ShareAUnit(row, col, row1, col1)
                    && CoordinateUtils.ShareAUnit(row, col, row2, col2))
                {
                    yield return new Coordinate(row, col); 
                }
            }
        }
    }

    public IEnumerable<Coordinate> SharedSeenEmptyCells(IStrategyManager strategyManager, int row1, int col1, int row2, int col2)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Sudoku[row, col] != 0 ||
                    (row == row1 && col == col1) || (row == row2 && col == col2)) continue;
                
                if (CoordinateUtils.ShareAUnit(row, col, row1, col1)
                    && CoordinateUtils.ShareAUnit(row, col, row2, col2))
                {
                    yield return new Coordinate(row, col);  
                }
            }
        }
    }
}