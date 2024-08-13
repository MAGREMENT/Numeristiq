using System;
using System.Collections.Generic;
using Model.Utility;

namespace Model.Core.Generators;

/// <summary>
/// RDR = Random Digit Removal
/// </summary>
public abstract class RDRPuzzleGenerator<T> : IPuzzleGenerator<T> where T : ICellsAndDigitsPuzzle
{
    private readonly Random _random = new();
    
    public IFilledPuzzleGenerator<T> FilledGenerator { get; set; }
    public bool KeepSymmetry { get; set; }
    public bool KeepUniqueness { get; set; } = true;
    public event OnNextStep? StepDone;

    protected RDRPuzzleGenerator(IFilledPuzzleGenerator<T> filledGenerator)
    {
        FilledGenerator = filledGenerator;
    }

    public T Generate()
    {
        var filled = FilledGenerator.Generate(out var list);
        StepDone?.Invoke(StepType.FilledGenerated);

        return RemoveRandomDigits(filled, list);
    }

    public T[] Generate(int count)
    {
        var result = new T[count];

        for (int i = 0; i < count; i++)
        {
            result[i] = Generate();
            StepDone?.Invoke(StepType.PuzzleGenerated);
        }

        return result;
    }
    
    protected abstract int GetSolutionCount(T puzzle, int stopAt);
    protected abstract Cell GetSymmetricCell(T puzzle, Cell cell);

    private T RemoveRandomDigits(T filled, List<Cell> list)
    {
        while (list.Count > 0)
        {
            var i = _random.Next(list.Count);
            var cell = list[i];
            list.RemoveAt(i);

            if (KeepSymmetry)
            {
                var otherCell = GetSymmetricCell(filled, cell);
                if (list.Remove(otherCell)) TryRemove(filled, cell, otherCell);
                else TryRemove(filled, cell);
            }
            else TryRemove(filled, cell);
        }

        return filled;
    }

    private void TryRemove(T filled, Cell cell)
    {
        var n = filled[cell.Row, cell.Column];
        filled[cell.Row, cell.Column] = 0;
        if (!IsValid(filled)) filled[cell.Row, cell.Column] = n;
    }

    private void TryRemove(T filled, params Cell[] cells)
    {
        var buffer = new int[cells.Length];
        for (int i = 0; i < cells.Length; i++)
        {
            buffer[i] = filled[cells[i].Row, cells[i].Column];
            filled[cells[i].Row, cells[i].Column] = 0;
        }

        if (IsValid(filled)) return;
        
        for (int i = 0; i < cells.Length; i++)
        {
            filled[cells[i].Row, cells[i].Column] = buffer[i];
        }
    }

    private bool IsValid(T puzzle)
    {
        if (KeepUniqueness)
        {
            return GetSolutionCount(puzzle, 2) == 1;
        }

        return GetSolutionCount(puzzle, 1) > 0;
    }
}

public interface ICellsAndDigitsPuzzle
{
    int this[int row, int col] { get; set; }
}