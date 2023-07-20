using System.Collections.Generic;

namespace Model.DeprecatedStrategies.SamePossibilities;

public class MiniGridSamePossibilitiesStrategy : IStrategy
{
    public string Name { get; } = "Same possibility";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Easy;
    
    public void ApplyOnce(ISolverView solverView)
    {
        for(int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                foreach (var keyValuePair in GetDictionaryOfPossibilities(solverView, miniRow, miniCol))
                {
                    if (keyValuePair.Key.Count == keyValuePair.Value)
                        RemovePossibilitiesFromMiniGrid(solverView, miniRow, miniCol, keyValuePair.Key);
                }
            }
        }
    }

    private Dictionary<IPossibilities, int> GetDictionaryOfPossibilities(ISolverView solverView, int miniRow, int miniCol)
    {
        Dictionary<IPossibilities, int> result = new();
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                int realRow = miniRow * 3 + row;
                int realCol = miniCol * 3 + col;
                if (solverView.Sudoku[realRow, realCol] == 0)
                {
                    if (!result.TryAdd(solverView.Possibilities[realRow, realCol], 1))
                        result[solverView.Possibilities[realRow, realCol]] += 1;
                }
            }
        }

        return result;
    }

    private void RemovePossibilitiesFromMiniGrid(ISolverView solverView, int miniRow, int miniCol, IPossibilities toRemove)
    {
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                int realRow = miniRow * 3 + row;
                int realCol = miniCol * 3 + col;
                
                if (solverView.Sudoku[realRow, realCol] == 0 && !solverView.Possibilities[realRow, realCol].Equals(toRemove))
                {
                    foreach (var number in toRemove)
                    {
                        solverView.RemovePossibility(number, realRow, realCol, this);
                    }
                }
            }
        }
    }
}