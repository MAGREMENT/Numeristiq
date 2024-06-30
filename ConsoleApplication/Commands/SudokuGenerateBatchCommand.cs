using ConsoleApplication.Commands.Abstracts;
using Model.Core;
using Model.Core.Generators;
using Model.Core.Trackers;
using Model.Sudokus;
using Model.Sudokus.Generator;
using Model.Sudokus.Solver;

namespace ConsoleApplication.Commands;

public class SudokuGenerateBatchCommand : GenerateBatchCommand<Sudoku, ISudokuSolvingState>
{
    public SudokuGenerateBatchCommand() : base("Sudoku", new RDRSudokuPuzzleGenerator(new BackTrackingFilledSudokuGenerator()))
    {
    }

    protected override ITrackerAttachableSolver<ISudokuSolvingState> GetSolver(ArgumentInterpreter interpreter)
    {
        return interpreter.Instantiator.InstantiateSudokuSolver();
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