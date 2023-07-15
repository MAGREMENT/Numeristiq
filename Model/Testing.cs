using System;
using Model.Strategies.ChainingStrategiesUtil;

namespace Model;

public class Testing
{
    public static void Main(string[] args)
    {
        long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        foreach (var coord in new Coordinate(4, 2).SharedSeenCells(new Coordinate(1, 2)))
        {
            Console.WriteLine(coord);
        }

        long end = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        
        Console.WriteLine($"Time taken : {end - start}ms");
    }

    private static void PositionsTest()
    {
        Positions one = new();
        one.Add(0);
        one.Add(8);
        one.Add(5);

        Positions two = new();
        two.Add(2);
        two.Add(3);
        two.Add(4);
        two.Add(5);

        PrintPositions(one);
        PrintPositions(two);
        PrintPositions(one.Mash(two));
    }

    private static void PrintPositions(Positions pos)
    {
        Console.WriteLine("Count : " + pos.Count);
        foreach (var n in pos)
        {
            Console.WriteLine("Has : " + n);
        }
    }

    private void SudokuResolutionTest(String asString)
    {
        var sud = new Sudoku(asString);
        Console.WriteLine("Sudoku initial : ");
        Console.WriteLine(sud);

        var solver = new Solver(sud);
        int numbersAdded = 0;
        solver.NumberAdded += (_, _) => numbersAdded++;
        solver.Solve();
        Console.WriteLine("Sudoku après résolution : ");
        Console.WriteLine(solver.Sudoku);
        Console.WriteLine("Chiffres ajoutés : " + numbersAdded);
        Console.WriteLine("Est correct ? : " + sud.IsCorrect());
    }
}