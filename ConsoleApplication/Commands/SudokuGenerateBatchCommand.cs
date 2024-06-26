using ConsoleApplication.Commands.Abstracts;
using Model.Core;
using Model.Core.Generators;
using Model.Core.Trackers;
using Model.Sudokus;
using Model.Sudokus.Generator;
using Model.Sudokus.Solver;

namespace ConsoleApplication.Commands;

public class SudokuGenerateBatchCommand : GenerateBatchCommand<Sudoku>
{
    public SudokuGenerateBatchCommand() : base("Sudoku", new RDRSudokuPuzzleGenerator(new BackTrackingFilledSudokuGenerator()))
    {
    }

    protected override (ISolver, IRatingTracker, IHardestStrategyTracker) GetSolverWithAttachedTracker(ArgumentInterpreter interpreter)
    {
        var solver = interpreter.Instantiator.InstantiateSudokuSolver();
        var ratings = new RatingTracker();
        var hardest = new HardestStrategyTracker();

        ratings.AttachTo(solver);
        hardest.AttachTo(solver);

        return (solver, ratings, hardest);
    }

    protected override GeneratedPuzzle<Sudoku> CreateGeneratedPuzzle(Sudoku puzzle)
    {
        return new GeneratedSudokuPuzzle(puzzle);
    }

    protected override void SetPuzzle(ISolver solver, Sudoku puzzle)
    {
        ((SudokuSolver)solver).SetSudoku(puzzle);
    }
}