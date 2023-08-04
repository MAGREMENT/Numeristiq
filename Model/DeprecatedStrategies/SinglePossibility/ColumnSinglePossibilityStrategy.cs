namespace Model.DeprecatedStrategies.SinglePossibility;

public class ColumnSinglePossibilityStrategy : IStrategy
{
    public string Name { get; } = "Single possibility";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Basic;
    public int Score { get; set; }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int col = 0; col < 9; col++)
        {
            for (int n = 1; n <= 9; n++)
            {
                int pos = CheckColumnForUnique(strategyManager, col, n);
                if (pos != -1)
                {
                    strategyManager.AddDefinitiveNumber(n, pos, col, this);
                }
            } 
        }
    }

    public string GetExplanation(IChangeCauseFactory factory)
    {
        throw new System.NotImplementedException();
    }

    private int CheckColumnForUnique(IStrategyManager strategyManager, int col, int number)
    {
        int buffer = -1;

        for (int i = 0; i < 9; i++)
        {
            if (strategyManager.Sudoku[i, col] == number) return -1;
            if (strategyManager.Possibilities[i, col].Peek(number) && strategyManager.Sudoku[i, col] == 0)
            {
                if (buffer != -1) return -1;
                buffer = i;
            }
        }

        return buffer;
    }
}