using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using Model.StrategiesV2;

namespace Model;

public class Testing
{
    public static void Main(string[] args)
    {
        long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        long end = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        
        Console.WriteLine($"Time taken : {end - start}ms");
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