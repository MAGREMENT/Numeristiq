using System;
using System.Collections.Generic;
using System.Printing;

namespace Model;

public class Testing
{
    public static void Main(string[] args)
    {
        var sud = new Sudoku("s4s6  27s8s5  4 91 8   8s5s4   43s5s7  8  3 3s4s9  172 1s6s9s4s2");
        Solver solv = new Solver(sud);
        solv.Solve();
        Console.WriteLine(solv.Possibilities[1, 6]);
        solv.Solve();
        Console.WriteLine(solv.Possibilities[1, 6]);
    }

    private void SudokuResolutionTest(String asString)
    {
        var sud = new Sudoku(asString);
        Console.WriteLine("Sudoku initial : ");
        Console.WriteLine(sud);

        var solver = new Solver(sud);
        int numbersAdded = 0;
        solver.NumberAdded += () => numbersAdded++;
        solver.Solve();
        Console.WriteLine("Sudoku après résolution : ");
        Console.WriteLine(solver.Sudoku);
        Console.WriteLine("Chiffres ajoutés : " + numbersAdded);
        Console.WriteLine("Est correct ? : " + sud.IsCorrect());
    }
}