using System.Linq;

namespace Model.DeprecatedStrategies.SinglePossibility;

public class CellSinglePossibilityStrategy : IStrategy
{
    public string Name { get; } = "Single possibility";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Basic;
    public int Score { get; set; }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int i = 0; i < 9; i++) 
        {
            for (int j = 0; j < 9; j++)
            {
                if (strategyManager.Sudoku[i, j] != 0) continue;
                
                if (strategyManager.Possibilities[i, j].Count == 1)
                {
                    int n = strategyManager.Possibilities[i, j].GetFirst();
                    strategyManager.AddDefinitiveNumber(n,
                        i, j, this);
                }
            }
        }
    }
}