using System;
using System.Collections.Generic;
using Model.Sudoku.Solver.StrategiesUtility;

namespace Model.Sudoku.Generator;

/// <summary>
/// RCR = Random Cell Removal
/// </summary>
public class RCRSudokuPuzzleGenerator : ISudokuPuzzleGenerator
{
    private readonly Random _random = new();
    private readonly IFilledSudokuGenerator _filledGenerator;

    public RCRSudokuPuzzleGenerator(IFilledSudokuGenerator filledGenerator)
    {
        _filledGenerator = filledGenerator;
    }
    
    public Sudoku Generate(OnFilledSudokuGenerated action)
    {
        var filled = _filledGenerator.Generate();
        action();

        return RemoveRandomDigits(filled);
    }

    public Sudoku Generate()
    {
        var filled = _filledGenerator.Generate();

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
        var list = new List<int>(81);
        for (int i = 0; i < 81; i++) list.Add(i);

        while (list.Count > 0)
        {
            var i = _random.Next(list.Count);

            var row = list[i] / 9;
            var col = list[i] % 9;

            list.RemoveAt(i);

            var n = sudoku[row, col];
            sudoku[row, col] = 0;
            if (BackTracking.Fill(sudoku.Copy(), ConstantPossibilitiesGiver.Instance, 2).Length != 1)
            {
                sudoku[row, col] = n;
            }
        }

        return sudoku;
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