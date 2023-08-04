using System.Linq;

namespace Model.Strategies;

public class BoxLineReduction : IStrategy
{
    public string Name => "Box line reduction";
    public StrategyLevel Difficulty => StrategyLevel.Medium;
    public int Score { get; set; }
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int number = 1; number <= 9; number++)
            {
                var ppir = strategyManager.PossibilityPositionsInRow(row, number);
                if (ppir.AreAllInSameMiniGrid())
                {
                    var changeBuffer =
                        strategyManager.CreateChangeBuffer(this, new RowLinePositionsCauseFactory(row, ppir, number));
                    
                    int miniRow = row / 3;
                    int miniCol = ppir.First() / 3;

                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            int realRow = miniRow * 3 + r;
                            int realCol = miniCol * 3 + c;

                            if (realRow != row) changeBuffer.AddPossibilityToRemove(number, realRow, realCol);
                        }
                    }
                    
                    changeBuffer.Push();
                }
            }
        }
        
        for (int col = 0; col < 9; col++)
        {
            for (int number = 1; number <= 9; number++)
            {
                var ppic = strategyManager.PossibilityPositionsInColumn(col, number);
                if (ppic.AreAllInSameMiniGrid())
                {
                    var changeBuffer =
                        strategyManager.CreateChangeBuffer(this, new ColumnLinePositionsCauseFactory(col, ppic, number));
                    
                    int miniRow = ppic.First() / 3;
                    int miniCol = col / 3;

                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            int realRow = miniRow * 3 + r;
                            int realCol = miniCol * 3 + c;

                            if (realCol != col) changeBuffer.AddPossibilityToRemove(number, realRow, realCol);
                        }
                    }
                    
                    changeBuffer.Push();
                }
            }
        }
    }
}