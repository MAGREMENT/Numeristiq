using System.Collections.Generic;
using Model.Solver;
using Model.Solver.Helpers;
using Model.Solver.Possibilities;

namespace Model.DeprecatedStrategies.SamePossibilities;

public class RowSamePossibilitiesStrategy : IStrategy
{
    public string Name { get; } = "Same possibility";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Easy;
    public StatisticsTracker Tracker { get; } = new();

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            foreach (var keyValuePair in GetDictionaryOfPossibilities(strategyManager, row))
            {
                if (keyValuePair.Key.Count == keyValuePair.Value)
                    RemovePossibilitiesFromRow(strategyManager, row, keyValuePair.Key);
            }
        }
    }

    private Dictionary<IReadOnlyPossibilities, int> GetDictionaryOfPossibilities(IStrategyManager strategyManager, int row)
    {
        Dictionary<IReadOnlyPossibilities, int> result = new();
        for (int col = 0; col < 9; col++)
        {
            if (strategyManager.Sudoku[row, col] == 0)
            {
                if (!result.TryAdd(strategyManager.PossibilitiesAt(row, col), 1))
                    result[strategyManager.PossibilitiesAt(row, col)] += 1;
            }
        }

        return result;
    }

    private void RemovePossibilitiesFromRow(IStrategyManager strategyManager, int row, IReadOnlyPossibilities toRemove)
    {
        for (int col = 0; col < 9; col++)
        {
            if (strategyManager.Sudoku[row, col] == 0 && !strategyManager.PossibilitiesAt(row, col).Equals(toRemove))
            {
                foreach (var number in toRemove)
                {
                    strategyManager.RemovePossibility(number, row, col, this);
                }
            }
        }
    }

}