using ConsoleApplication.Commands.Abstracts;
using Model.Core;
using Model.Sudokus;
using Model.Sudokus.Solver;

namespace ConsoleApplication.Commands;

public class SudokuSolveCommand : SolveCommand
{
    public SudokuSolveCommand() : base("Sudoku")
    {
    }

    protected override ISolver GetSolverAndSetPuzzle(ArgumentInterpreter interpreter, string puzzle)
    {
        var solver = interpreter.Instantiator.InstantiateSudokuSolver();
        solver.SetSudoku(SudokuTranslator.TranslateLineFormat(puzzle));

        return solver;
    }

    protected override string PuzzleAsString(ISolver solver)
    {
        return ((SudokuSolver)solver).Sudoku.ToString()!;
    }
}