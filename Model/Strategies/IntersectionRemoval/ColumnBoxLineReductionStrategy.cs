using System.Linq;

namespace Model.Strategies.IntersectionRemoval;

public class ColumnBoxLineReductionStrategy : IStrategy
{
    public string Name { get; } = "Box line reduction";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Medium;
    
    public void ApplyOnce(ISolverView solverView)
    {
        for (int col = 0; col < 9; col++)
        {
            for (int number = 1; number <= 9; number++)
            {
                var ppic = solverView.PossibilityPositionsInColumn(col, number);
                if (ppic.AreAllInSameMiniGrid())
                {
                    int miniRow = ppic.First() / 3;
                    int miniCol = col / 3;

                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            int realRow = miniRow * 3 + r;
                            int realCol = miniCol * 3 + c;

                            if (realCol != col && solverView.Sudoku[realRow, realCol] == 0)
                                solverView.RemovePossibility(number, realRow, realCol, this);
                        }
                    }
                }
            }
        }
    }
}