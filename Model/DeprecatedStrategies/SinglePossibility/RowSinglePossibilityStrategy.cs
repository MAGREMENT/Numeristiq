namespace Model.DeprecatedStrategies.SinglePossibility;

public class RowSinglePossibilityStrategy : IStrategy
{
    public string Name { get; } = "Single possibility";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Basic;
    public int Score { get; set; }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int n = 1; n <= 9; n++)
            {
                int pos = CheckRowForUnique(strategyManager, row, n);
                if (pos != -1)
                {
                    strategyManager.AddDefinitiveNumber(n, row, pos, this);
                }
            } 
        }
    }

    public string GetExplanation(IChangeCauseFactory factory)
    {
        throw new System.NotImplementedException();
    }

    private int CheckRowForUnique(IStrategyManager strategyManager, int row, int number)
    {
        int buffer = -1;

        for (int i = 0; i < 9; i++)
        {
            if (strategyManager.Sudoku[row, i] == number) return -1;
            if (strategyManager.Possibilities[row, i].Peek(number) && strategyManager.Sudoku[row, i] == 0)
            {
                if (buffer != -1) return -1;
                buffer = i;
            }
        }

        return buffer;
    }
}