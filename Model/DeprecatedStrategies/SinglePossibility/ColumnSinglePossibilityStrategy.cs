namespace Model.DeprecatedStrategies.SinglePossibility;

public class ColumnSinglePossibilityStrategy : IStrategy
{
    public string Name { get; } = "Single possibility";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Basic;
    
    public void ApplyOnce(ISolverView solverView)
    {
        for (int col = 0; col < 9; col++)
        {
            for (int n = 1; n <= 9; n++)
            {
                int pos = CheckColumnForUnique(solverView, col, n);
                if (pos != -1)
                {
                    solverView.AddDefinitiveNumber(n, pos, col, this);
                }
            } 
        }
    }
    
    private int CheckColumnForUnique(ISolverView solverView, int col, int number)
    {
        int buffer = -1;

        for (int i = 0; i < 9; i++)
        {
            if (solverView.Sudoku[i, col] == number) return -1;
            if (solverView.Possibilities[i, col].Peek(number) && solverView.Sudoku[i, col] == 0)
            {
                if (buffer != -1) return -1;
                buffer = i;
            }
        }

        return buffer;
    }
}