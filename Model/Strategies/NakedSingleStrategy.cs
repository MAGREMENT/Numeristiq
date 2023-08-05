using System.Linq;

namespace Model.Strategies;

public class NakedSingleStrategy : IStrategy
{
    public string Name => "Naked single";
    public StrategyLevel Difficulty => StrategyLevel.Basic;
    public int Score { get; set; }
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Possibilities[row, col].Count == 1) strategyManager.AddDefinitiveNumber(
                        strategyManager.Possibilities[row, col].First(), row, col, this);
            }
        }
    }
}