using Model.Core.BackTracking;
using Model.Core.Generators;
using Model.Utility;

namespace Model.Sudokus.Generator;

public class RDRSudokuPuzzleGenerator : RDRPuzzleGenerator<Sudoku>
{
    private readonly SudokuBackTracker _backTracker = new();
    
    public RDRSudokuPuzzleGenerator(IFilledPuzzleGenerator<Sudoku> filledGenerator) : base(filledGenerator)
    {
    }

    protected override int GetSolutionCount(Sudoku puzzle, int stopAt)
    {
        _backTracker.StopAt = stopAt;
        _backTracker.Set(puzzle, ConstantPossibilitiesGiver.Nine);
        return _backTracker.Count();
    }

    protected override Cell GetSymmetricCell(Sudoku puzzle,Cell cell)
    {
        return new Cell(8 - cell.Row, 8 - cell.Column);
    }
}