using System;
using System.Collections.Generic;
using Model.Core.Generators;
using Model.Utility;

namespace Model.Sudokus.Generator;

/// <summary>
/// RDR = Random Digit Removal
/// </summary>
public class RDRSudokuPuzzleGenerator : RDRPuzzleGenerator<Sudoku>
{
    public RDRSudokuPuzzleGenerator(IFilledPuzzleGenerator<Sudoku> filledGenerator) : base(filledGenerator)
    {
    }

    protected override int GetSolutionCount(Sudoku puzzle, int stopAt)
    {
        return BackTracking.Count(puzzle, ConstantPossibilitiesGiver.Instance, stopAt);
    }

    protected override Cell GetSymmetricCell(Sudoku puzzle,Cell cell)
    {
        return new Cell(8 - cell.Row, 8 - cell.Column);
    }
}

public class ConstantPossibilitiesGiver : IPossibilitiesGiver
{
    private static readonly IEnumerable<int> _enumerable = Create();

    public static ConstantPossibilitiesGiver Instance { get; } = new();

    private ConstantPossibilitiesGiver()
    {
        
    }
    
    public IEnumerable<int> EnumeratePossibilitiesAt(int row, int col)
    {
        return _enumerable;
    }

    private static IEnumerable<int> Create()
    {
        for (int i = 1; i <= 9; i++)
        {
            yield return i;
        }
    }
}