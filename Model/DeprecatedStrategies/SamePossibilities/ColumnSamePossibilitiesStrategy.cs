using System.Collections.Generic;

namespace Model.DeprecatedStrategies.SamePossibilities;

public class ColumnSamePossibilitiesStrategy : IStrategy
{
    public void ApplyOnce(ISolver solver)
    {
        for (int col = 0; col < 9; col++)
        {
            foreach (var keyValuePair in GetDictionaryOfPossibilities(solver, col))
            {
                if (keyValuePair.Key.Count == keyValuePair.Value)
                    RemovePossibilitiesFromColumn(solver, col, keyValuePair.Key);
            }
        }
    }

    private Dictionary<IPossibilities, int> GetDictionaryOfPossibilities(ISolver solver, int col)
    {
        Dictionary<IPossibilities, int> result = new();
        for (int row = 0; row < 9; row++)
        {
            if (solver.Sudoku[row, col] == 0)
            {
                if (!result.TryAdd(solver.Possibilities[row, col], 1))
                    result[solver.Possibilities[row, col]] += 1;
            }
        }

        return result;
    }

    private void RemovePossibilitiesFromColumn(ISolver solver, int col, IPossibilities toRemove)
    {
        for (int row = 0; row < 9; row++)
        {
            if (solver.Sudoku[row, col] == 0 && !solver.Possibilities[row, col].Equals(toRemove))
            {
                foreach (var number in toRemove)
                {
                    solver.RemovePossibility(number, row, col,
                            new SamePossibilitiesLog(number, row, col));
                }
            }
        }
    }
}