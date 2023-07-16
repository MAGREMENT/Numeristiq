using System.Collections.Generic;

namespace Model.DeprecatedStrategies.SamePossibilities;

public class RowSamePossibilitiesStrategy : IStrategy
{
    public void ApplyOnce(ISolver solver)
    {
        for (int row = 0; row < 9; row++)
        {
            foreach (var keyValuePair in GetDictionaryOfPossibilities(solver, row))
            {
                if (keyValuePair.Key.Count == keyValuePair.Value)
                    RemovePossibilitiesFromRow(solver, row, keyValuePair.Key);
            }
        }
    }
    
    private Dictionary<IPossibilities, int> GetDictionaryOfPossibilities(ISolver solver, int row)
    {
        Dictionary<IPossibilities, int> result = new();
        for (int col = 0; col < 9; col++)
        {
            if (solver.Sudoku[row, col] == 0)
            {
                if (!result.TryAdd(solver.Possibilities[row, col], 1))
                    result[solver.Possibilities[row, col]] += 1;
            }
        }

        return result;
    }

    private void RemovePossibilitiesFromRow(ISolver solver, int row, IPossibilities toRemove)
    {
        for (int col = 0; col < 9; col++)
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