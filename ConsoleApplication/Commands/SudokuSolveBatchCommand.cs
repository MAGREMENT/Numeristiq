using ConsoleApplication.Commands.Abstracts;
using Model.Core;
using Model.Sudokus;
using Model.Sudokus.Solver;

namespace ConsoleApplication.Commands;

public class SudokuSolveBatchCommand : SolveBatchCommand<ISudokuSolvingState>
{
    public SudokuSolveBatchCommand() : base("Sudoku")
    {
    }

    protected override ITrackerAttachableSolver<ISudokuSolvingState> GetSolver(Instantiator instantiator)
    {
        return instantiator.InstantiateSudokuSolver();
    }

    protected override bool Set(ISolver solver, string asString)
    {
        ((SudokuSolver)solver).SetSudoku(SudokuTranslator.TranslateLineFormat(asString));
        return true;
    }
}