using System;
using System.Collections.Generic;
using Model.Sudokus.Solver.Utility;
using Model.Utility;

namespace Model.Sudokus.Generator;

public class BackTrackingFilledSudokuGenerator : IFilledSudokuGenerator
{
    public Sudoku Generate()
    {
        return BackTracking.Fill(new Sudoku(), new RandomPossibilitiesGiver(), 1)[0];
    }

    public List<int> GetRemovableCells()
    {
        var list = new List<int>(81);
        for (int i = 0; i < 81; i++)
        {
            list.Add(i);
        }

        return list;
    }
}

public class RandomPossibilitiesGiver : IPossibilitiesGiver
{
    private readonly Random _random = new();
    
    public IEnumerable<int> EnumeratePossibilitiesAt(int row, int col)
    {
        var list = new List<int>(9) { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        while (list.Count > 0)
        {
            var i = _random.Next(list.Count);
            yield return list[i];
            list.RemoveAt(i);
        }
    }
}