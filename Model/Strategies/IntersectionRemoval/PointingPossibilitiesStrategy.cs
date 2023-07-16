using System.Linq;

namespace Model.Strategies.IntersectionRemoval;

public class PointingPossibilitiesStrategy : IStrategy
{
    public void ApplyOnce(ISolver solver)
    {
        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                for (int number = 1; number <= 9; number++)
                {
                    var ppimg = solver.PossibilityPositionsInMiniGrid(miniRow, miniCol, number);
                    if (ppimg.AreAllInSameRow())
                    {
                        int row = ppimg.First()[0];
                        for (int col = 0; col < 9; col++)
                        {
                            if (col / 3 != miniCol && solver.Sudoku[row, col] == 0)
                                solver.RemovePossibility(number, row, col,
                                    new IntersectionRemovalLog(number, row, col));
                        }
                    }
                    else if (ppimg.AreAllInSameColumn())
                    {
                        int col = ppimg.First()[1];
                        for (int row = 0; row < 9; row++)
                        {
                            if (row / 3 != miniRow && solver.Sudoku[row, col] == 0)
                                solver.RemovePossibility(number, row, col,
                                    new IntersectionRemovalLog(number, row, col));
                        }
                    }
                }
            }
        }
    }
}