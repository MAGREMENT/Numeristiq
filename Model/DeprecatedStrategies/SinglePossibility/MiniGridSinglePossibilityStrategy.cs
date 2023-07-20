namespace Model.DeprecatedStrategies.SinglePossibility;

public class MiniGridSinglePossibilityStrategy : IStrategy
{
    public string Name { get; } = "Single possibility";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Basic;
    
    public void ApplyOnce(ISolverView solverView)
    {
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                for (int n = 1; n <= 9; n++)
                {
                    int[]? pos = CheckMiniGridForUnique(solverView, gridRow, gridCol, n);
                    if (pos is not null)
                    {
                        solverView.AddDefinitiveNumber(n, pos[0], pos[1], this);
                    }
                }
            }
        }
    }
    
    private int[]? CheckMiniGridForUnique(ISolverView solverView, int gridRow, int gridCol, int number)
    {
        int[]? pos = null;

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                var realRow = gridRow * 3 + row;
                var realCol = gridCol * 3 + col;

                if (solverView.Sudoku[realRow, realCol] == number) return null;
                if (solverView.Possibilities[realRow, realCol].Peek(number) && solverView.Sudoku[realRow, realCol] == 0)
                {
                    if (pos is not null) return null;
                    pos = new[] { realRow, realCol };
                }
            }
        }

        return pos;
    }
}