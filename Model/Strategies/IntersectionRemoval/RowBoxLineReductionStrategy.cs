using System.Collections.Generic;
using System.Linq;

namespace Model.Strategies.IntersectionRemoval;

public class RowBoxLineReductionStrategy : IStrategy
{
    public void ApplyOnce(ISolver solver)
    {
        bool wasProgressMade = false;

        for (int row = 0; row < 9; row++)
        {
            for (int number = 1; number <= 9; number++)
            {
                var ppir = solver.PossibilityPositionsInRow(row, number);
                if (ppir.Count is > 1 and < 4)
                {
                    if (IsInSameMiniGrid(ppir.All()))
                    {
                        int miniRow = row / 3;
                        int miniCol = ppir.All().First() / 3;

                        for (int r = 0; r < 3; r++)
                        {
                            for (int c = 0; c < 3; c++)
                            {
                                int realRow = miniRow * 3 + r;
                                int realCol = miniCol * 3 + c;

                                if (realRow != row && solver.Sudoku[realRow, realCol] == 0 &&
                                    solver.RemovePossibility(number, realRow, realCol,
                                        new IntersectionRemovalLog(number, realRow, realCol))) wasProgressMade = true;
                            }
                        }
                    }
                }
            }
        }
    }

    private bool IsInSameMiniGrid(IEnumerable<int> list)
    {
        int miniCol = list.First() / 3;

        foreach(int col in list)
        {
            if (col / 3 != miniCol) return false;
        }

        return true;
    }
}