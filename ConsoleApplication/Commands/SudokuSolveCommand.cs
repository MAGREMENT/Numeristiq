using ConsoleApplication.Commands.Abstracts;
using Model.Core;
using Model.Core.Highlighting;
using Model.Sudokus;
using Model.Sudokus.Solver;

namespace ConsoleApplication.Commands;

public class SudokuSolveCommand : SolveCommand<SudokuStrategy, IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    public SudokuSolveCommand() : base("Sudoku")
    {
    }

    protected override NumericStrategySolver<SudokuStrategy, IUpdatableSudokuSolvingState, ISudokuHighlighter> GetSolverAndSetPuzzle(ArgumentInterpreter interpreter, string puzzle)
    {
        var solver = interpreter.Instantiator.InstantiateSudokuSolver();
        solver.SetSudoku(SudokuTranslator.TranslateLineFormat(puzzle));

        return solver;
    }

    protected override string PuzzleAsString(NumericStrategySolver<SudokuStrategy, IUpdatableSudokuSolvingState, ISudokuHighlighter> solver)
    {
        return ((SudokuSolver)solver).Sudoku.ToString()!;
    }
}