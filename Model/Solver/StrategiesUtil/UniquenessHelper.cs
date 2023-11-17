using Model.Solver.Position;

namespace Model.Solver.StrategiesUtil;

public static class UniquenessHelper
{
    public static int CheckForSoloRow(GridPositions gp)
    {
        var result = -1;

        for (int row = 0; row < 9; row++)
        {
            var count = gp.RowCount(row);
            if (count is 0 or 2) continue;

            if (count == 1)
            {
                if (result == -1) result = row;
                else return -1;
            }
            else return -1;
        }

        return result;
    }
    
    public static int CheckForSoloColumn(GridPositions gp)
    {
        var result = -1;

        for (int col = 0; col < 9; col++)
        {
            var count = gp.ColumnCount(col);
            if (count is 0 or 2) continue;

            if (count == 1)
            {
                if (result == -1) result = col;
                else return -1;
            }
            else return -1;
        }

        return result;
    }

    public static int CheckForSoloMini(GridPositions gp)
    {
        var result = -1;

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                var count = gp.MiniGridCount(miniRow, miniCol);
                if (count is 0 or 2) continue;

                if (count == 1)
                {
                    if (result == -1) result = miniRow * 3 + miniCol;
                    else return -1;
                }
                else return -1;
            }
        }

        return result;
    }
}