using System.Collections.Generic;
using Model.Sudokus.Solver.Position;

namespace Model.Sudokus.Solver.Utility;

public static class BackTracking
{
    public static Sudoku[] Fill(Sudoku start, IPossibilitiesGiver giver, int stopAt)
    {
        var result = new SudokuBackTrackingListResult();
        Start(result, start, giver, stopAt);
        return result.ToArray();
    }

    public static int Count(Sudoku start, IPossibilitiesGiver giver, int stopAt)
    {
        var result = new SudokuBackTrackingCountResult();
        Start(result, start, giver, stopAt);
        return result.Count;
    }
    
    private static void Start(ISudokuBackTrackingResult result, Sudoku start, IPossibilitiesGiver giver, int stopAt)
    {
        var positions = new GridPositions[]
        {
            new(), new(), new(), new(), new(), new(), new(), new(), new()
        };

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
    }
    
    
    private static bool Search(ISudokuBackTrackingResult result, Sudoku current, IPossibilitiesGiver giver,
        IReadOnlyList<GridPositions> positions, int position, int stopAt)
    {
        if (position == 81)
        {
            result.AddNewResult(current.Copy());
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

public interface ISudokuBackTrackingResult
{
    void AddNewResult(Sudoku sudoku);
    int Count { get; }
}

public class SudokuBackTrackingListResult : List<Sudoku>, ISudokuBackTrackingResult
{
    public void AddNewResult(Sudoku sudoku)
    {
        Add(sudoku.Copy());
    }
}

public class SudokuBackTrackingCountResult : ISudokuBackTrackingResult
{
    public void AddNewResult(Sudoku sudoku)
    {
        Count++;
    }

    public int Count { get; private set; }
}

