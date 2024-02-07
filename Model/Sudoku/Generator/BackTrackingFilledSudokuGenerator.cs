using System;
using System.Collections.Generic;
using Model.Sudoku.Solver.StrategiesUtility;

namespace Model.Sudoku.Generator;

public class BackTrackingFilledSudokuGenerator : IFilledSudokuGenerator
{
    public Sudoku Generate()
    {
        return BackTracking.Fill(new Sudoku(), new RandomPossibilitiesGiver(), 1)[0];
    }
}

public class RandomPossibilitiesGiver : IPossibilitiesGiver
{
    private readonly Random _random = new();
    
    public IEnumerable<int> EnumeratePossibilitiesAt(int row, int col)
    {
        List<int> list = new List<int>(9) { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        while (list.Count > 0)
        {
            var i = _random.Next(list.Count);
            yield return list[i];
            list.RemoveAt(i);
        }
    }
}