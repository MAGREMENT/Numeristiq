using System.Collections.Generic;
using Model.Core.Generators;
using Model.Core.Settings;
using Model.Core.Syntax;
using Model.Utility;
using Model.Utility.Collections;

namespace Model.YourPuzzles;

public class NumericYourPuzzle : IReadOnlyNumericYourPuzzle
{
    private NumericCell[,] _cells;
    private readonly List<IGlobalNumericPuzzleRule> _global;
    private readonly List<ILocalNumericPuzzleRule> _local;

    public int RowCount => _cells.GetLength(0);
    public int ColumnCount => _cells.GetLength(1);

    public IReadOnlyList<IGlobalNumericPuzzleRule> GlobalRules => _global;
    public IReadOnlyList<ILocalNumericPuzzleRule> LocalRules => _local;
    
    public NumericYourPuzzle(int rowCount, int colCount)
    {
        _cells = new NumericCell[rowCount, colCount];
        _global = new List<IGlobalNumericPuzzleRule>();
        _local = new List<ILocalNumericPuzzleRule>();
        for (int r = 0; r < rowCount; r++)
        {
            for (int c = 0; c < colCount; c++)
            {
                _cells[r, c] = new NumericCell();
            }
        }
    }

    public void AddRuleUnchecked(ILocalNumericPuzzleRule rule)
    {
        foreach (var cell in rule.EnumerateCells())
        {
            _cells[cell.Row, cell.Column].AddRule(rule);
        }
        
        _local.Add(rule);
    }
    
    public void AddRuleUnchecked(IGlobalNumericPuzzleRule rule)
    {
        _global.Add(rule);
    }
    
    public void AddRuleUnchecked(INumericPuzzleRule rule)
    {
        switch (rule)
        {
            case ILocalNumericPuzzleRule l : AddRuleUnchecked(l);
                break;
            case IGlobalNumericPuzzleRule g : AddRuleUnchecked(g);
                break;
        }
    }
    
    public void RemoveRule(int index, bool isGlobal)
    {
        if (isGlobal) _global.RemoveAt(index);
        else
        {
            var local = _local[index];
            _local.RemoveAt(index);
            
            foreach (var cell in local.EnumerateCells())
            {
                _cells[cell.Row, cell.Column].RemoveRule(local);
            }
        }
    }

    public void DisableCell(int row, int col)
    {
        var cell = _cells[row, col];
        if (!cell.IsEnabled) return;

        foreach (var rule in cell.Rules)
        {
            _local.Remove(rule);
        }
        
        cell.Disable();
    }

    public void EnableCell(int row, int col)
    {
        var cell = _cells[row, col];
        if (!cell.IsEnabled) cell.Value = 0;
    }

    public bool IsEnabled(int row, int col)
    {
        return _cells[row, col].IsEnabled;
    }

    public bool IsCorrect()
    {
        foreach (var rule in _global)
        {
            if (!rule.IsRespected(this)) return false;
        }
        
        foreach (var rule in _local)
        {
            if (!rule.IsRespected(this)) return false;
        }

        return true;
    }
    
    public bool AreAllEnabled(IEnumerable<Cell> cells)
    {
        foreach (var cell in cells)
        {
            if (!_cells[cell.Row, cell.Column].IsEnabled) return false;
        }

        return true;
    }

    public bool AreAllEnabled()
    {
        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColumnCount; col++)
            {
                if (!_cells[row, col].IsEnabled) return false;
            }
        }

        return true;
    }

    public void ChangeSize(int rowCount, int colCount)
    {
        if (rowCount == RowCount && colCount == ColumnCount) return;
        
        var newCells = new NumericCell[rowCount, colCount];
        for (int r = 0; r < rowCount; r++)
        {
            for (int c = 0; c < colCount; c++)
            {
                newCells[r, c] = r >= RowCount || c >= ColumnCount ? new NumericCell() : _cells[r, c];
            }
        }

        _local.RemoveAll(rule => !rule.IsStillApplicable(rowCount, colCount));
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
    public int RowCount { get; }
    public int ColumnCount { get; }
    
    public int this[Cell cell] { get; set; }
    public IReadOnlyList<IGlobalNumericPuzzleRule> GlobalRules { get; }
    public IReadOnlyList<ILocalNumericPuzzleRule> LocalRules { get; }

    bool AreAllEnabled(IEnumerable<Cell> cells);
    bool AreAllEnabled();
    bool IsEnabled(int row, int col);
    bool IsEnabled(Cell cell) => IsEnabled(cell.Row, cell.Column);
}

public class NumericCell
{
    private readonly List<ILocalNumericPuzzleRule> _rules = new();
    
    public int Value { get; set; }
    public IReadOnlyList<ILocalNumericPuzzleRule> Rules => _rules;

    public void Disable()
    {
        Value = -1;
        _rules.Clear();
    }

    public bool IsEnabled => Value != -1;

    public void AddRule(ILocalNumericPuzzleRule rule) => _rules.Add(rule);
    public void RemoveRule(ILocalNumericPuzzleRule rule) => _rules.Remove(rule);
}

public interface INumericPuzzleRule : INamed, ISyntaxTranslatable
{ 
    string Abbreviation { get; }
    IEnumerable<ISetting> EnumerateSettings();
    bool IsRespected(IReadOnlyNumericYourPuzzle board);
    string DataToString();
}

public interface IGlobalNumericPuzzleRule : INumericPuzzleRule
{
    
}

public interface ILocalNumericPuzzleRule : INumericPuzzleRule
{
    IEnumerable<Cell> EnumerateCells();
    bool IsStillApplicable(int rowCount, int colCount);
    static bool DefaultIsStillApplicable(ILocalNumericPuzzleRule rule, int rowCount, int colCount)
    {
        foreach (var cell in rule.EnumerateCells())
        {
            if (cell.Row >= rowCount || cell.Column >= colCount) return false;
        }

        return true;
    }
}