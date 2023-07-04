using System.Collections.Generic;

namespace Model.Strategies.SamePossibilities;

public class RowSamePossibilitiesStrategy : ISubStrategy
{
    public bool ApplyOnce(ISolver solver)
    {
        bool wasProgressMade = false;
        
        for (int row = 0; row < 9; row++)
        {
            foreach (var keyValuePair in GetDictionaryOfPossibilities(solver, row))
            {
                if (keyValuePair.Key.Count == keyValuePair.Value)
                    wasProgressMade = RemovePossibilitiesFromRow(solver, row, keyValuePair.Key);
            }
        }

        return wasProgressMade;
    }
    
    private Dictionary<CellPossibilities, int> GetDictionaryOfPossibilities(ISolver solver, int row)
    {
        Dictionary<CellPossibilities, int> result = new();
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

    private bool RemovePossibilitiesFromRow(ISolver solver, int row, CellPossibilities toRemove)
    {
        bool wasProgressMade = false;
        
        for (int col = 0; col < 9; col++)
        {
            if (solver.Sudoku[row, col] == 0 && !solver.Possibilities[row, col].Equals(toRemove))
            {
                foreach (var number in toRemove.GetPossibilities())
                {
                    if (solver.RemovePossibility(number, row, col,
                            new SamePossibilitiesLog(number, row, col))) wasProgressMade = true;
                }
            }
        }

        return wasProgressMade;
    }

}