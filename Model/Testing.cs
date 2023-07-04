using System;
using System.Collections.Generic;
using System.Printing;

namespace Model;

public class Testing
{
    public static void Main(string[] args)
    {
        var sud = new Sudoku("938761245567942183241853976124687539896435721753129468682594317379216854415378692");
        Console.WriteLine(sud.IsCorrect());
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