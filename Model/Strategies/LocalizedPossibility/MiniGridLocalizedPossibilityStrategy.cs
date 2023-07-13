using System.Collections.Generic;

namespace Model.Strategies.LocalizedPossibility;

public class MiniGridLocalizedPossibilityStrategy : ISubStrategy
{
    public bool ApplyOnce(ISolver solver)
    {
        bool wasProgressMade = false;
        
        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                for (int number = 1; number <= 9; number++)
                {
                    var ppimg = solver.PossiblePositionsInMiniGrid(miniRow, miniCol, number);
                    if (ppimg.Count is > 1 and < 4)
                    {
                        if (HasSameRow(ppimg))
                        {
                            int row = ppimg[0][0];
                            for (int col = 0; col < 9; col++)
                            {
                                if (col / 3 != miniCol && solver.Sudoku[row, col] == 0 &&
                                    solver.RemovePossibility(number, row, col,
                                        new LocalizedPossibilityLog(number, row, col))) wasProgressMade = true;
                            }
                        }else if (HasSameColumn(ppimg))
                        {
                            int col = ppimg[0][1];
                            for (int row = 0; row < 9; row++)
                            {
                                if (row / 3 != miniRow && solver.Sudoku[row, col] == 0 &&
                                    solver.RemovePossibility(number, row, col,
                                        new LocalizedPossibilityLog(number, row, col))) wasProgressMade = true;
                            }
                        }
                    }
                }
            }
        }

        return wasProgressMade;
    }

    private bool HasSameRow(List<int[]> list)
    {
        int row = list[0][0];
        for (int i = 1; i < list.Count; i++)
        {
            if (list[i][0] != row) return false;
        }

        return true;
    }
    
    private bool HasSameColumn(List<int[]> list)
    {
        int row = list[0][1];
        for (int i = 1; i < list.Count; i++)
        {
            if (list[i][1] != row) return false;
        }

        return true;
    }
}