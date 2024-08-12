using System;
using System.Collections.Generic;
using Model.Core.BackTracking;
using Model.Core.Generators;
using Model.Utility;

namespace Model.Sudokus.Generator;

/// <summary>
/// RDR = Random Digit Removal
/// </summary>
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