﻿using System;
using System.Collections.Generic;
using Model.Helpers;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Kakuros;

public class ArrayKakuro : IKakuro, ISolvingState
{
    private KakuroCell[,] _cells;
    private readonly List<IKakuroSum> _sums = new();

    public int RowCount => _cells.GetLength(0);
    public int ColumnCount => _cells.GetLength(1);
    public IReadOnlyList<IKakuroSum> Sums => _sums;

    public ArrayKakuro()
    {
        _cells = new KakuroCell[0, 0];
    }

    public ArrayKakuro(int rowCount, int colCount)
    {
        _cells = new KakuroCell[rowCount, colCount];
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
            if(cell.Row >= RowCount || cell.Column >= ColumnCount) continue;

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
        var fr = sum.GetFarthestRow();
        var fc = sum.GetFarthestColumn();

        if (fr >= RowCount || fc >= ColumnCount)
        {
            var buffer = new KakuroCell[Math.Max(RowCount, fr + 1), Math.Max(ColumnCount, fc + 1)];
            for (int row = 0; row < RowCount; row++)
            {
                for (int col = 0; col < ColumnCount; col++)
                {
                    buffer[row, col] = _cells[row, col];
                }
            }
            _cells = buffer;
        }

        _sums.Add(sum);
        foreach (var cell in sum)
        {
            if (sum.Orientation == Orientation.Vertical) _cells[cell.Row, cell.Column] += sum;
            else _cells[cell.Row, cell.Column] -= sum;
        }
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

    public int Number { get; }
    public IKakuroSum? VerticalSum { get; }
    public IKakuroSum? HorizontalSum { get; }

    public static KakuroCell operator +(KakuroCell cell, int n) => new(n, cell.VerticalSum, cell.HorizontalSum);
    public static KakuroCell operator +(KakuroCell cell, IKakuroSum? v) => new(cell.Number, v, cell.HorizontalSum);
    public static KakuroCell operator -(KakuroCell cell, IKakuroSum? h) => new(cell.Number, cell.VerticalSum, h);
}