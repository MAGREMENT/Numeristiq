using System.Linq;

namespace Model.Strategies;

public class PointingPossibilitiesStrategy : IStrategy
{
    public string Name { get; } = "Pointing possibilities";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Medium;
    public int Score { get; set; }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                for (int number = 1; number <= 9; number++)
                {
                    var ppimg = strategyManager.PossibilityPositionsInMiniGrid(miniRow, miniCol, number);
                    if (ppimg.AreAllInSameRow())
                    {
                        var changeBuffer = strategyManager.CreateChangeBuffer(this,
                            new MiniGridPositionsCauseFactory(ppimg, number));
                        int row = ppimg.First()[0];
                        for (int col = 0; col < 9; col++)
                        {
                            if (col / 3 != miniCol) changeBuffer.AddPossibilityToRemove(number, row, col);
                        }
                        
                        changeBuffer.Push();
                    }
                    else if (ppimg.AreAllInSameColumn())
                    {
                        var changeBuffer = strategyManager.CreateChangeBuffer(this,
                            new MiniGridPositionsCauseFactory(ppimg, number));
                        int col = ppimg.First()[1];
                        for (int row = 0; row < 9; row++)
                        {
                            if (row / 3 != miniRow) changeBuffer.AddPossibilityToRemove(number, row, col);
                        }
                        
                        changeBuffer.Push();
                    }
                }
            }
        }
    }

    public string GetExplanation(IChangeCauseFactory factory)
    {
        return "";
    }
}