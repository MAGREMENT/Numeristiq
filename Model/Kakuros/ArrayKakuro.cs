using System;
using System.Collections.Generic;
using Model.Core;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Kakuros;

public class ArrayKakuro : IKakuro, ISolvingState
{
    private KakuroCell[,] _cells;
    private readonly List<IKakuroSum> _sums;

    public int RowCount => _cells.GetLength(0);
    public int ColumnCount => _cells.GetLength(1);

    public IReadOnlyList<IKakuroSum> Sums => _sums;

    public ArrayKakuro()
    {
        _cells = new KakuroCell[0, 0];
        _sums = new List<IKakuroSum>();
    }

    public ArrayKakuro(int rowCount, int colCount)
    {
        _cells = new KakuroCell[rowCount, colCount];
        _sums = new List<IKakuroSum>();
    }

    private ArrayKakuro(KakuroCell[,] cells, List<IKakuroSum> sums)
    {
        _cells = cells;
        _sums = sums;
    }
    
    public int GetSolutionCount()
    {
        int total = 0;
        foreach (var cell in _cells)
        {
            if (cell.Number != 0) total++;
        }

        return total;
    }

    public int GetCellCount()
    {
        int total = 0;
        foreach (var cell in _cells)
        {
            if (cell.IsUsed()) total++;
        }

        return total;
    }
    
    public IEnumerable<IKakuroSum> SumsFor(Cell cell)
    {
        var c = _cells[cell.Row, cell.Column];
        if (c.VerticalSum is not null) yield return c.VerticalSum;
        if (c.HorizontalSum is not null) yield return c.HorizontalSum;
    }

    public IKakuroSum? VerticalSumFor(Cell cell)
    {
        return _cells[cell.Row, cell.Column].VerticalSum;
    }

    public IKakuroSum? HorizontalSumFor(Cell cell)
    {
        return _cells[cell.Row, cell.Column].HorizontalSum;
    }

    public List<int> GetSolutions(IKakuroSum sum)
    {
        List<int> result = new();
        foreach (var cell in sum)
        {
            var n = this[cell.Row, cell.Column];
            if (n != 0) result.Add(n);
        }

        return result;
    }

    public bool AddSum(IKakuroSum sum)
    {
        if (KakuroCellUtility.MaxAmountFor(sum.Length) < sum.Amount) return false;
        
        var amountCell = sum.GetAmountCell();
        if (amountCell is { Row: >= 0, Column: >= 0 } && amountCell.Row < RowCount &&
            amountCell.Column < ColumnCount && _cells[amountCell.Row, amountCell.Column].IsUsed()) return false;
        
        foreach (var cell in sum)
        {
            if (cell.Row < 0 || cell.Column < 0) return false;
            if (cell.Row >= RowCount || cell.Column >= ColumnCount) continue;

            if (sum.Orientation == Orientation.Horizontal)
            {
                if (HorizontalSumFor(cell) is not null) return false;
            }
            else if (VerticalSumFor(cell) is not null) return false;
        }

        AddSumUnchecked(sum);
        return true;
    }

    public void AddSumUnchecked(IKakuroSum sum)
    {
        _sums.Add(sum);
        UpdateAfterAddition(sum);
    }

    public IKakuroSum? FindSum(Cell amountCell)
    {
        var cell = new Cell(amountCell.Row + 1, amountCell.Column);
        if (cell is { Row: >= 0, Column: >= 0 } && cell.Row < RowCount && cell.Column < ColumnCount)
        {
            var sum = _cells[cell.Row, cell.Column].VerticalSum;
            if (sum is not null) return sum;
        }

        cell = new Cell(amountCell.Row, amountCell.Column + 1);
        if (cell.Row < 0 || cell.Column < 0 || cell.Row >= RowCount || cell.Column <= ColumnCount) return null;

        return _cells[cell.Row, cell.Column].HorizontalSum;
    }

    public int this[int row, int col]
    {
        get => _cells[row, col].Number;
        set => _cells[row, col] += value;
    }

    public bool IsComplete()
    {
        foreach (var cell in _cells)
        {
            if (cell.IsUsed() && cell.Number == 0) return false;
        }

        return true;
    }

    public IKakuro Copy()
    {
        var buffer = new KakuroCell[RowCount, ColumnCount];
        Array.Copy(_cells, buffer, _cells.Length);

        return new ArrayKakuro(buffer, new List<IKakuroSum>(_sums));
    }

    public bool AddCellTo(IKakuroSum sum) //TODO case where there is a sum below
    {
        if (sum.Length >= 9) return false;

        var index = _sums.IndexOf(sum);
        if (index == -1) return false;

        var newSum = sum.WithLength(sum.Length + 1);
        _sums[index] = newSum;
        UpdateAfterAddition(newSum);

        var (c1, c2) = newSum.GetFarthestCellPerpendicularNeighbors();
        IKakuroSum? s1 = null, s2 = null;

        if (c1.Row >= 0 && c1.Row < RowCount && c1.Column >= 0 && c1.Column < ColumnCount)
        {
            s1 = newSum.Orientation == Orientation.Horizontal
                ? _cells[c1.Row, c1.Column].VerticalSum
                : _cells[c1.Row, c1.Column].HorizontalSum;
        }
        
        if (c2.Row >= 0 && c2.Row < RowCount && c2.Column >= 0 && c2.Column < ColumnCount)
        {
            s2 = newSum.Orientation == Orientation.Horizontal
                ? _cells[c2.Row, c2.Column].VerticalSum
                : _cells[c2.Row, c2.Column].HorizontalSum;
        }

        if (s1 is not null)
        {
            var additionalLength = 0;
            if (s2 is not null)
            {
                additionalLength = s2.Length;
                _sums.Remove(s2);
            }
            
            var newS1 = s1.WithLength(s1.Length + 1 + additionalLength);
            _sums[_sums.IndexOf(s1)] = newS1;
            ReplaceCellSums(newS1);
        }
        else if (s2 is not null)
        {
            var newS2 = s2.MoveBack(1);
            _sums[_sums.IndexOf(s2)] = newS2;
            ReplaceCellSums(newS2);
        }

        return true;
    }

    public bool RemoveCell(Cell cell)
    {
        if (!_cells[cell.Row, cell.Column].IsRemovable) return false;
        
        foreach (var sum in SumsFor(cell))
        {
            var index = _sums.IndexOf(sum);
            if (index == -1) continue;

            var sums = sum.DivideAround(cell);

            if (sums.Item1 is not null)
            {
                _sums[index] = sums.Item1;
                ReplaceCellSums(sums.Item1);
            }
            else _sums.RemoveAt(index);

            if (sums.Item2 is null)
            {
                var fr = 0;
                var fc = 0;
                foreach (var s in _sums) //TODO only do in the sum direction
                {
                    fr = Math.Max(s.GetFarthestRow(), fr);
                    fc = Math.Max(s.GetFarthestColumn(), fc);
                }

                if (fr != RowCount || fc != ColumnCount)
                {
                    ResizeTo(fr + 1, fc + 1);
                    //Resize done, no need to remove the cell manually
                    return true;
                }
            }
            else
            {
                _sums.Add(sums.Item2);
                ReplaceCellSums(sums.Item2);
            }
        }
        
        _cells[cell.Row, cell.Column] += null;
        _cells[cell.Row, cell.Column] -= null;

        return true;
    }

    public bool ReplaceAmount(IKakuroSum sum, int amount)
    {
        var index = _sums.IndexOf(sum);
        if (index == -1) return false;

        _sums[index] = _sums[index].WithAmount(amount);
        return true;
    }

    public ReadOnlyBitSet16 PossibilitiesAt(int row, int col)
    {
        return new ReadOnlyBitSet16();
    }

    public IEnumerable<Cell> EnumerateCells()
    {
        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColumnCount; col++)
            {
                if (_cells[row, col].IsUsed()) yield return new Cell(row, col);
            }
        }
    }

    private void UpdateAfterAddition(IKakuroSum sum)
    {
        var fr = sum.GetFarthestRow();
        var fc = sum.GetFarthestColumn();

        if (fr >= RowCount || fc >= ColumnCount)
        {
            ResizeTo(Math.Max(RowCount, fr + 1), Math.Max(ColumnCount, fc + 1));
        }

        ReplaceCellSums(sum);
    }

    private void ReplaceCellSums(IKakuroSum sum)
    {
        foreach (var cell in sum)
        {
            if (sum.Orientation == Orientation.Vertical) _cells[cell.Row, cell.Column] += sum;
            else _cells[cell.Row, cell.Column] -= sum;
        }
    }

    private void ResizeTo(int rowCount, int colCount)
    {
        var buffer = new KakuroCell[rowCount, colCount];
        for (int row = 0; row < RowCount && row < rowCount; row++)
        {
            for (int col = 0; col < ColumnCount && col < colCount; col++)
            {
                buffer[row, col] = _cells[row, col];
            }
        }
        _cells = buffer;
    }
}

public readonly struct KakuroCell
{
    public KakuroCell()
    {
        Number = 0;
        VerticalSum = null;
        HorizontalSum = null;
    }
    
    public KakuroCell(int number, IKakuroSum? verticalSum, IKakuroSum? horizontalSum)
    {
        Number = number;
        VerticalSum = verticalSum;
        HorizontalSum = horizontalSum;
    }

    public bool IsUsed() => VerticalSum is not null || HorizontalSum is not null;

    public bool IsRemovable => (VerticalSum is not null && VerticalSum.Length > 1) || (HorizontalSum is not null
        && HorizontalSum.Length > 1);

    public int Number { get; }
    public IKakuroSum? VerticalSum { get; }
    public IKakuroSum? HorizontalSum { get; }

    public static KakuroCell operator +(KakuroCell cell, int n) => new(n, cell.VerticalSum, cell.HorizontalSum);
    public static KakuroCell operator +(KakuroCell cell, IKakuroSum? v) => new(cell.Number, v, cell.HorizontalSum);
    public static KakuroCell operator -(KakuroCell cell, IKakuroSum? h) => new(cell.Number, cell.VerticalSum, h);
}