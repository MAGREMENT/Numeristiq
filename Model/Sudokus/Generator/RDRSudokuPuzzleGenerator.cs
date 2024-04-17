using System;
using System.Collections.Generic;
using Model.Sudokus.Solver.StrategiesUtility;
using Model.Utility;

namespace Model.Sudokus.Generator;

/// <summary>
/// RDR = Random Digit Removal
/// </summary>
public class RDRSudokuPuzzleGenerator : ISudokuPuzzleGenerator
{
    private readonly Random _random = new();
    
    public IFilledSudokuGenerator FilledGenerator { get; set; }
    public bool KeepSymmetry { get; set; }
    public bool KeepUniqueness { get; set; } = true;

    public RDRSudokuPuzzleGenerator(IFilledSudokuGenerator filledGenerator)
    {
        FilledGenerator = filledGenerator;
    }
    
    public Sudoku Generate(OnFilledSudokuGenerated action)
    {
        var filled = FilledGenerator.Generate();
        action();

        return RemoveRandomDigits(filled);
    }

    public Sudoku Generate()
    {
        var filled = FilledGenerator.Generate();

        return RemoveRandomDigits(filled);
    }

    public Sudoku[] Generate(int count)
    {
        var result = new Sudoku[count];

        for (int i = 0; i < count; i++)
        {
            result[i] = Generate();
        }

        return result;
    }

    private Sudoku RemoveRandomDigits(Sudoku sudoku)
    {
        var list = FilledGenerator.GetRemovableCells();

        while (list.Count > 0)
        {
            var i = _random.Next(list.Count);

            var row = list[i] / 9;
            var col = list[i] % 9;

            list.RemoveAt(i);

            if (KeepSymmetry)
            {
                var r2 = 8 - row;
                var c2 = 8 - col;
                if (list.Remove(r2 * 9 + c2)) TryRemove(sudoku, new Cell(row, col), new Cell(r2, c2));
                else TryRemove(sudoku, row, col);
            }
            else TryRemove(sudoku, row, col);
        }

        return sudoku;
    }

    private void TryRemove(Sudoku sudoku, int row, int col)
    {
        var n = sudoku[row, col];
        sudoku[row, col] = 0;
        if (!IsSudokuValid(sudoku)) sudoku[row, col] = n;
    }

    private void TryRemove(Sudoku sudoku, params Cell[] cells)
    {
        var buffer = new int[cells.Length];
        for (int i = 0; i < cells.Length; i++)
        {
            buffer[i] = sudoku[cells[i].Row, cells[i].Column];
            sudoku[cells[i].Row, cells[i].Column] = 0;
        }

        if (IsSudokuValid(sudoku)) return;
        
        for (int i = 0; i < cells.Length; i++)
        {
            sudoku[cells[i].Row, cells[i].Column] = buffer[i];
        }
    }

    private bool IsSudokuValid(IReadOnlySudoku sudoku)
    {
        if (KeepUniqueness) return BackTracking.Fill(sudoku.Copy(),
                ConstantPossibilitiesGiver.Instance, 2).Length == 1;

        return BackTracking.Fill(sudoku.Copy(), ConstantPossibilitiesGiver.Instance, 1).Length > 0;
    }
}

public delegate void OnFilledSudokuGenerated();

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