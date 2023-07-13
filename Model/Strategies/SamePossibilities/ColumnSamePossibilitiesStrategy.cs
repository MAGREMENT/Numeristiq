using System.Collections.Generic;

namespace Model.Strategies.SamePossibilities;

public class ColumnSamePossibilitiesStrategy : ISubStrategy
{
    public bool ApplyOnce(ISolver solver)
    {
        bool wasProgressMade = false;
        
        for (int col = 0; col < 9; col++)
        {
            foreach (var keyValuePair in GetDictionaryOfPossibilities(solver, col))
            {
                if (keyValuePair.Key.Count == keyValuePair.Value)
                    wasProgressMade = RemovePossibilitiesFromColumn(solver, col, keyValuePair.Key);
                else if (keyValuePair.Key.Count == 3 && keyValuePair.Value == 2)
                {
                    
                }
            }
        }

        return wasProgressMade;
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

    private bool RemovePossibilitiesFromColumn(ISolver solver, int col, IPossibilities toRemove)
    {
        bool wasProgressMade = false;
        
        for (int row = 0; row < 9; row++)
        {
            if (solver.Sudoku[row, col] == 0 && !solver.Possibilities[row, col].Equals(toRemove))
            {
                foreach (var number in toRemove.All())
                {
                    if (solver.RemovePossibility(number, row, col,
                            new SamePossibilitiesLog(number, row, col))) wasProgressMade = true;
                }
            }
        }

        return wasProgressMade;
    }

    private bool SearchForHiddenTriple(ISolver solver, int col, IPossibilities search)
    {
        return false;
    }
}