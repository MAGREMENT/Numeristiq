using System;
using System.IO;
using System.Text;
using Model.Strategies.ChainingStrategiesUtil;

namespace Model;

public class Testing
{
    public static void Main(string[] args)
    {
        long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        FullSudokuBankTest();

        long end = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        
        Console.WriteLine($"Time taken : {end - start}ms");
    }

    private static void FullSudokuBankTest()
    {
        try
        {
            using TextReader reader =
                new StreamReader("C:\\Users\\Zach\\Desktop\\Perso\\SudokuSolver\\Model\\SudokuBank.txt", Encoding.UTF8);
            while (reader.ReadLine() is { } line)
            {
                Solver solver = new(new Sudoku(line));
                solver.Solve();
                
                if(!solver.Sudoku.IsCorrect()) Console.WriteLine("WRONG ! => " + line);
                else Console.WriteLine("OK!");
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
        Positions one = new()
        {
            0,
            8,
            5
        };

        Positions two = new()
        {
            2,
            3,
            4,
            5
        };

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