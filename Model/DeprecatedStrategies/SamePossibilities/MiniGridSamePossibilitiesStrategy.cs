using System.Collections.Generic;
using Model.Possibilities;

namespace Model.DeprecatedStrategies.SamePossibilities;

public class MiniGridSamePossibilitiesStrategy : IStrategy
{
    public string Name { get; } = "Same possibility";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Easy;
    public int Score { get; set; }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for(int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                foreach (var keyValuePair in GetDictionaryOfPossibilities(strategyManager, miniRow, miniCol))
                {
                    if (keyValuePair.Key.Count == keyValuePair.Value)
                        RemovePossibilitiesFromMiniGrid(strategyManager, miniRow, miniCol, keyValuePair.Key);
                }
            }
        }
    }

    private Dictionary<IPossibilities, int> GetDictionaryOfPossibilities(IStrategyManager strategyManager, int miniRow, int miniCol)
    {
        Dictionary<IPossibilities, int> result = new();
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                int realRow = miniRow * 3 + row;
                int realCol = miniCol * 3 + col;
                if (strategyManager.Sudoku[realRow, realCol] == 0)
                {
                    if (!result.TryAdd(strategyManager.Possibilities[realRow, realCol], 1))
                        result[strategyManager.Possibilities[realRow, realCol]] += 1;
                }
            }
        }

        return result;
    }

    private void RemovePossibilitiesFromMiniGrid(IStrategyManager strategyManager, int miniRow, int miniCol, IPossibilities toRemove)
    {
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                int realRow = miniRow * 3 + row;
                int realCol = miniCol * 3 + col;
                
                if (strategyManager.Sudoku[realRow, realCol] == 0 && !strategyManager.Possibilities[realRow, realCol].Equals(toRemove))
                {
                    foreach (var number in toRemove)
                    {
                        strategyManager.RemovePossibility(number, realRow, realCol, this);
                    }
                }
            }
        }
    }
}