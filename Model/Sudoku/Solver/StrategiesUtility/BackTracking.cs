using System.Collections.Generic;
using Model.Sudoku.Solver.Position;

namespace Model.Sudoku.Solver.StrategiesUtility;

public static class BackTracking
{
    public static Sudoku[] Fill(Sudoku start, IPossibilitiesGiver giver, int stopAt)
    {
        List<Sudoku> result = new();
        
        var positions = new GridPositions[] { new(), new(), new(), new(), new(), new(), new(), new(), new() };

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var n = start[row, col];
                if (n == 0) continue;

                positions[n - 1].Add(row, col);
            }
        }
        
        Search(result, start, giver, positions, 0, stopAt);

        return result.ToArray();
    }
    
    private static bool Search(List<Sudoku> result, Sudoku current, IPossibilitiesGiver giver,
        GridPositions[] positions, int position, int stopAt)
    {
        if (position == 81)
        {
            result.Add(current.Copy());
            return result.Count >= stopAt;
        }
        
        var row = position / 9;
        var col = position % 9;
        
        if (current[row, col] != 0)
        {
            if (Search(result, current, giver, positions, position + 1, stopAt)) return true;
        }
        else
        {
            foreach (var possibility in giver.EnumeratePossibilitiesAt(row, col))
            {
                var pos = positions[possibility - 1];
                if(pos.IsRowNotEmpty(row) || pos.IsColumnNotEmpty(col) 
                                          || pos.IsMiniGridNotEmpty(row / 3, col / 3)) continue;
                
                current[row, col] = possibility;
                pos.Add(row, col);

                if (Search(result, current, giver, positions, position + 1, stopAt)) return true;
                
                current[row, col] = 0;
                pos.Remove(row, col);
            }
        }

        return false;
    }
}

public interface IPossibilitiesGiver
{
    IEnumerable<int> EnumeratePossibilitiesAt(int row, int col);
}