namespace Model.DeprecatedStrategies.SinglePossibility;

public class RowSinglePossibilityStrategy : IStrategy
{
    public string Name { get; } = "Single possibility";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Basic;
    
    public void ApplyOnce(ISolverView solverView)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int n = 1; n <= 9; n++)
            {
                int pos = CheckRowForUnique(solverView, row, n);
                if (pos != -1)
                {
                    solverView.AddDefinitiveNumber(n, row, pos, this);
                }
            } 
        }
    }
    
    private int CheckRowForUnique(ISolverView solverView, int row, int number)
    {
        int buffer = -1;

        for (int i = 0; i < 9; i++)
        {
            if (solverView.Sudoku[row, i] == number) return -1;
            if (solverView.Possibilities[row, i].Peek(number) && solverView.Sudoku[row, i] == 0)
            {
                if (buffer != -1) return -1;
                buffer = i;
            }
        }

        return buffer;
    }
}