using System;
using System.Collections.Generic;
using Model.Core.Generators;
using Model.Sudokus.Solver.Utility;
using Model.Utility;

namespace Model.Sudokus.Generator;

public class BackTrackingFilledSudokuGenerator : IFilledPuzzleGenerator<Sudoku>
{
    public Sudoku Generate()
    {
        return BackTracking.Solutions(new Sudoku(), new RandomPossibilitiesGiver(), 1)[0];
    }

    public Sudoku Generate(out List<Cell> removableCells)
    {
        removableCells = GetRemovableCells();
        return Generate();
    }

    private static List<Cell> GetRemovableCells()
    {
        var list = new List<Cell>(81);
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                list.Add(new Cell(row, col));
            }
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