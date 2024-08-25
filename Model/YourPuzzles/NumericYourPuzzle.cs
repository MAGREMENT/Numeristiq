using System.Collections.Generic;
using Model.Core.Generators;
using Model.Utility;

namespace Model.YourPuzzles;

public class NumericYourPuzzle : IReadOnlyNumericYourPuzzle
{
    private NumericCell[,] _cells;
    private readonly List<INumericPuzzleRule> _rules;

    public int RowCount => _cells.GetLength(0);
    public int ColumnCount => _cells.GetLength(1);

    public NumericYourPuzzle(int rowCount, int colCount)
    {
        _cells = new NumericCell[rowCount, colCount];
        _rules = new List<INumericPuzzleRule>();
        for (int r = 0; r < rowCount; r++)
        {
            for (int c = 0; c < colCount; c++)
            {
                _cells[r, c] = new NumericCell();
            }
        }
    }

    public bool AddRule(INumericPuzzleRule rule)
    {
        foreach (var cell in rule.EnumerateCells())
        {
            if (!_cells[cell.Row, cell.Column].IsEnabled) return false;
        }

        _rules.Add(rule);
        foreach (var cell in rule.EnumerateCells())
        {
            _cells[cell.Row, cell.Column].AddRule(rule);
        }

        return true;
    }

    public void DisableCell(int row, int col)
    {
        var cell = _cells[row, col];
        if (!cell.IsEnabled) return;

        foreach (var rule in cell.Rules)
        {
            _rules.Remove(rule);
        }
        
        cell.Disable();
    }

    public void EnableCell(int row, int col)
    {
        var cell = _cells[row, col];
        if (!cell.IsEnabled) cell.Value = 0;
    }

    public bool IsCorrect()
    {
        foreach (var rule in _rules)
        {
            if (!rule.IsRespected(this)) return false;
        }

        return true;
    }

    public void ChangeSize(int rowCount, int colCount)
    {
        if (rowCount == RowCount && colCount == ColumnCount) return;
        
        var newCells = new NumericCell[rowCount, colCount];
        for (int r = 0; r < rowCount && r < RowCount; r++)
        {
            for (int c = 0; c < colCount && c < ColumnCount; c++)
            {
                newCells[r, c] = _cells[r, c];
            }
        }

        _rules.RemoveAll(rule => !rule.IsStillApplicable(rowCount, colCount));
        _cells = newCells;
    }

    public int this[int row, int col]
    {
        get => _cells[row, col].Value;
        set
        {
            var cell = _cells[row, col];
            if (cell.IsEnabled) cell.Value = value;
        }
    }

    public int this[Cell cell]
    {
        get => this[cell.Row, cell.Column];
        set => this[cell.Row, cell.Column] = value;
    }
}

public interface IReadOnlyNumericYourPuzzle : ICellsAndDigitsPuzzle
{
    public int this[Cell cell] { get; set; }
}

public class NumericCell
{
    private readonly List<INumericPuzzleRule> _rules = new();
    
    public int Value { get; set; }
    public IReadOnlyList<INumericPuzzleRule> Rules => _rules;

    public void Disable()
    {
        Value = -1;
        _rules.Clear();
    }

    public bool IsEnabled => Value != -1;

    public void AddRule(INumericPuzzleRule rule) => _rules.Add(rule);
}

public interface INumericPuzzleRule
{
    IEnumerable<Cell> EnumerateCells();

    bool IsRespected(IReadOnlyNumericYourPuzzle board);
    bool IsStillApplicable(int rowCount, int colCount);

    static bool DefaultIsStillApplicable(INumericPuzzleRule rule, int rowCount, int colCount)
    {
        foreach (var cell in rule.EnumerateCells())
        {
            if (cell.Row >= rowCount || cell.Column >= colCount) return false;
        }

        return true;
    }
}