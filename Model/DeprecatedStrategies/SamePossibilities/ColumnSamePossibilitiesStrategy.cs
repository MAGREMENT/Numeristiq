using System.Collections.Generic;
using Model.Possibilities;

namespace Model.DeprecatedStrategies.SamePossibilities;

public class ColumnSamePossibilitiesStrategy : IStrategy
{
    public string Name { get; } = "Same possibility";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Easy;

    public void ApplyOnce(ISolverView solverView)
    {
        for (int col = 0; col < 9; col++)
        {
            foreach (var keyValuePair in GetDictionaryOfPossibilities(solverView, col))
            {
                if (keyValuePair.Key.Count == keyValuePair.Value)
                    RemovePossibilitiesFromColumn(solverView, col, keyValuePair.Key);
            }
        }
    }

    private Dictionary<IPossibilities, int> GetDictionaryOfPossibilities(ISolverView solverView, int col)
    {
        Dictionary<IPossibilities, int> result = new();
        for (int row = 0; row < 9; row++)
        {
            if (solverView.Sudoku[row, col] == 0)
            {
                if (!result.TryAdd(solverView.Possibilities[row, col], 1))
                    result[solverView.Possibilities[row, col]] += 1;
            }
        }

        return result;
    }

    private void RemovePossibilitiesFromColumn(ISolverView solverView, int col, IPossibilities toRemove)
    {
        for (int row = 0; row < 9; row++)
        {
            if (solverView.Sudoku[row, col] == 0 && !solverView.Possibilities[row, col].Equals(toRemove))
            {
                foreach (var number in toRemove)
                {
                    solverView.RemovePossibility(number, row, col, this);
                }
            }
        }
    }
}