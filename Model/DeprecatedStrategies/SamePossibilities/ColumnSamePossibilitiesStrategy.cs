using System.Collections.Generic;
using Model.Solver;
using Model.Solver.Helpers;
using Model.Solver.Possibilities;

namespace Model.DeprecatedStrategies.SamePossibilities;

public class ColumnSamePossibilitiesStrategy : IStrategy
{
    public string Name => "Same possibility";
    public StrategyDifficulty Difficulty => StrategyDifficulty.Easy;
    public StatisticsTracker Tracker { get; } = new();

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int col = 0; col < 9; col++)
        {
            foreach (var keyValuePair in GetDictionaryOfPossibilities(strategyManager, col))
            {
                if (keyValuePair.Key.Count == keyValuePair.Value)
                    RemovePossibilitiesFromColumn(strategyManager, col, keyValuePair.Key);
            }
        }
    }

    private Dictionary<IReadOnlyPossibilities, int> GetDictionaryOfPossibilities(IStrategyManager strategyManager, int col)
    {
        Dictionary<IReadOnlyPossibilities, int> result = new();
        for (int row = 0; row < 9; row++)
        {
            if (strategyManager.Sudoku[row, col] == 0)
            {
                if (!result.TryAdd(strategyManager.PossibilitiesAt(row, col), 1))
                    result[strategyManager.PossibilitiesAt(row, col)] += 1;
            }
        }

        return result;
    }

    private void RemovePossibilitiesFromColumn(IStrategyManager strategyManager, int col, IReadOnlyPossibilities toRemove)
    {
        for (int row = 0; row < 9; row++)
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