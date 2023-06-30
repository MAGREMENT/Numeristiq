using System.Collections.Generic;

namespace Model.Strategies.IHaveToFindBetterNames;

public class RowDeductionStrategy : ISolverStrategy
{
    public bool ApplyOnce(ISolver solver)
    {
        bool wasProgressMade = false;

        for (int row = 0; row < 9; row++)
        {
            for (int number = 1; number <= 9; number++)
            {
                var ppir = PossiblePositionsInRow(solver, row, number);
                if (ppir.Count is > 1 and < 4)
                {
                    if (IsInSameMiniGrid(ppir))
                    {
                        int miniRow = ppir[0][0] / 3;
                        int miniCol = ppir[0][1] / 3;

                        for (int r = 0; r < 3; r++)
                        {
                            for (int c = 0; c < 3; c++)
                            {
                                int realRow = miniRow * 3 + r;
                                int realCol = miniCol * 3 + c;

                                if (realRow != row &&
                                    solver.RemovePossibility(number, realRow, realCol)) wasProgressMade = true;
                            }
                        }
                    }
                }
            }
        }

        return wasProgressMade;
    }

    private List<int[]> PossiblePositionsInRow(ISolver solver, int row, int number)
    {
        List<int[]> result = new();
        for (int col = 0; col < 9; col++)
        {
            if (solver.Sudoku[row, col] == number) return new List<int[]>();
            if (solver.Sudoku[row, col] == 0 &&
                solver.Possibilities[row, col].Peek(number)) result.Add(new[]{row, col});
        }

        return result;
    }

    private bool IsInSameMiniGrid(List<int[]> list)
    {
        int miniRow = list[0][0] / 3;
        int miniCol = list[0][1] / 3;

        for (int i = 1; i < list.Count; i++)
        {
            if (list[i][0] / 3 != miniRow || list[i][1] / 3 != miniCol) return false;
        }

        return true;
    }
}