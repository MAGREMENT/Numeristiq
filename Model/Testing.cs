using System;
using System.IO;
using System.Text;
using Model.Positions;
using Model.Strategies;
using Model.Strategies.StrategiesUtil;

namespace Model;

public static class Testing
{
    public static void Main(string[] args)
    {
        long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        CompareAicAlgorithms();

        long end = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        
        Console.WriteLine($"Time taken : {((double) end - start) / 1000}s");
    }

    private static void CompareAicAlgorithms()
    {
        long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        Solver solver =
            new Solver(new Sudoku("s4s7 38   628   99 8  5   7s5s8   53   42   4     5   3  6 43   267   91 4"));
        solver.Solve();
        AlternatingInferenceChainStrategy strat1 = (AlternatingInferenceChainStrategy)
            solver.GetStrategy(typeof(AlternatingInferenceChainStrategy));

        long end = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        
        Console.WriteLine("One------------------------------------------------");
        Console.WriteLine("Sudoku solved : " + solver.Sudoku.IsCorrect());
        Console.WriteLine($"Time taken : {end - start}ms");
        Console.WriteLine("Modification count : " + strat1.ModificationCount);
        Console.WriteLine("Search count : " + strat1.SearchCount);
        Console.WriteLine();
    }

    private static void CompareIPossibilitiesImplementation(IPossibilities one, IPossibilities two)
    {
        Console.WriteLine(one.Count);
        Console.WriteLine(two.Count);
        Console.WriteLine(one);
        Console.WriteLine(two);

        one.Remove(1);
        one.Remove(9);
        one.Remove(5);
        
        two.Remove(1);
        two.Remove(9);
        two.Remove(5);

        Console.WriteLine(one.Count);
        Console.WriteLine(two.Count);
        Console.WriteLine(one);
        Console.WriteLine(two);
        
        Console.WriteLine(one.Peek(2));
        Console.WriteLine(one.Peek(5));
        Console.WriteLine(one.Remove(1));
        Console.WriteLine(one.Remove(3));
        Console.WriteLine(two.Peek(2));
        Console.WriteLine(two.Peek(5));
        Console.WriteLine(two.Remove(1));
        Console.WriteLine(two.Remove(3));
        
        Console.WriteLine(one.Count);
        Console.WriteLine(two.Count);
        Console.WriteLine(one);
        Console.WriteLine(two);
    }

    private static void FullSudokuBankTest()
    {
        try
        {
            using TextReader reader =
                new StreamReader("C:\\Users\\Zach\\Desktop\\Perso\\SudokuSolver\\Model\\SudokuBank.txt", Encoding.UTF8);
            var counter = 1;
            while (reader.ReadLine() is { } line)
            {
                Solver solver = new(new Sudoku(line));
                solver.Solve();
                
                if(!solver.Sudoku.IsCorrect()) Console.WriteLine(counter++ + " WRONG ! => " + line);
                else Console.WriteLine(counter++ + " OK!");
            }
        }
        catch (IOException e)
        {
            Console.WriteLine("Reader problem : " + e.Message);
        }
    }

    private static void SharedSeenCellsTest()
    {
        foreach (var coord in new Coordinate(4, 2).SharedSeenCells(new Coordinate(1, 2)))
        {
            Console.WriteLine(coord);
        }
    }

    private static void PositionsTest()
    {
        LinePositions one = new()
        {
            0,
            8,
            5
        };

        LinePositions two = new()
        {
            2,
            3,
            4,
            5
        };

        MiniGridPositions three = new(1, 2);
        three.Add(0, 0); //3, 6
        three.Add(2, 2); //5, 8
        three.Add(1, 1); //4, 7
        three.Add(0, 2); //3, 8

        PrintPositions(one);
        PrintPositions(two);
        PrintPositions(one.Mash(two));
        PrintPositions(three);
    }

    private static void PrintPositions(LinePositions pos)
    {
        Console.WriteLine("Count : " + pos.Count);
        foreach (var n in pos)
        {
            Console.WriteLine("Has : " + n);
        }
    }

    private static void PrintPositions(MiniGridPositions pos)
    {
        Console.WriteLine("Count : " + pos.Count);
        foreach (var n in pos)
        {
            Console.WriteLine("Has : " + n[0] + ", " + n[1]);
        }
    }

    private static void SudokuResolutionTest(string asString)
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