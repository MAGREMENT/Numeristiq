using System;
using System.Collections;
using System.Collections.Generic;
using Model.Core;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Kakuros;

public class ArrayKakuro : IKakuro, ISolvingState
{
    private KakuroCell[,] _cells;
    private readonly List<IArrayKakuroSum> _sums;

    public int RowCount => _cells.GetLength(0);
    public int ColumnCount => _cells.GetLength(1);

    public IReadOnlyList<IKakuroSum> Sums => _sums;

    public ArrayKakuro()
    {
        _cells = new KakuroCell[0, 0];
        _sums = new List<IArrayKakuroSum>();
    }

    public ArrayKakuro(int rowCount, int colCount)
    {
        _cells = new KakuroCell[rowCount, colCount];
        _sums = new List<IArrayKakuroSum>();
    }

    private ArrayKakuro(KakuroCell[,] cells, List<IArrayKakuroSum> sums)
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
        }

        AddSumUnchecked(sum);
        return true;
    }

    public void AddSumUnchecked(IKakuroSum sum)
    {
        var kc = _cells[sum.GetStartCell().Row, sum.GetStartCell().Column];
        if (sum.Orientation == Orientation.Horizontal)
        {
            if (kc.HorizontalSum is not null && IKakuroSum.AreSame(kc.HorizontalSum, sum))
            {
                kc.HorizontalSum.SetAmount(sum.Amount);
                return;
            }
        }
        else if (kc.VerticalSum is not null && IKakuroSum.AreSame(kc.VerticalSum, sum))
        {
            kc.VerticalSum.SetAmount(sum.Amount);
            return;
        }
        
        var s = Cast(sum);
        _sums.Add(s);
        UpdateAfterAddition(s);
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
        if (cell.Row < 0 || cell.Column < 0 || cell.Row >= RowCount || cell.Column >= ColumnCount) return null;

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

        return new ArrayKakuro(buffer, new List<IArrayKakuroSum>(_sums));
    }

    public bool AddCellTo(IKakuroSum sum)
    {
        if (sum.Length >= 9) return false;

        var index = IndexOf(sum);
        if (index == -1) return false;

        var lengthAdded = 1;
        var cellForSumBelow = sum.GetFarthestCell(2);
        if (cellForSumBelow.Row < RowCount && cellForSumBelow.Column < ColumnCount)
        {
            var sumBelow = _cells[cellForSumBelow.Row, cellForSumBelow.Column].GetSum(sum.Orientation);
            if (sumBelow is not null)
            {
                lengthAdded += sumBelow.Length;
                _sums.RemoveAt(IndexOf(sumBelow));
            }
        }

        var newSum = _sums[index].WithLength(sum.Length + lengthAdded);
        _sums[index] = newSum;
        UpdateAfterAddition(newSum);

        var (c1, c2) = newSum.GetPerpendicularNeighbors(sum.Length);
        IArrayKakuroSum? s1 = null, s2 = null;

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

    public bool AddSumTo(Cell cell) //TODO fix when bottom
    {
        var kc = _cells[cell.Row, cell.Column];
        if (kc.HorizontalSum is null)
        {
            if (kc.VerticalSum is not null)
            {
                if (cell.Column > 0 && _cells[cell.Row, cell.Column - 1].IsUsed()) return false;

                var length = 1;
                while (cell.Column + length < ColumnCount && _cells[cell.Row, cell.Column + length].IsUsed())
                {
                    length++;
                }

                var sum = new HorizontalKakuroSum(cell, KakuroCellUtility.MinAmountFor(length), length);
                _sums.Add(sum);
                ReplaceCellSums(sum);

                return true;
            }
        }
        else if (kc.VerticalSum is null)
        {
            if (cell.Row > 0 && _cells[cell.Row - 1, cell.Row].IsUsed()) return false;

            var length = 1;
            while (cell.Row + length < RowCount && _cells[cell.Row + length, cell.Column].IsUsed())
            {
                length++;
            }

            var sum = new VerticalKakuroSum(cell, KakuroCellUtility.MinAmountFor(length), length);
            _sums.Add(sum);
            ReplaceCellSums(sum);

            return true;
        }

        return false;
    }

    public bool RemoveCell(Cell cell) //TODO shift up or left if needed
    {
        if (!_cells[cell.Row, cell.Column].IsRemovable()) return false;

        var stillNeedRemoval = true;
        
        foreach (var sum in SumsFor(cell))
        {
            var index = IndexOf(sum);
            if (index == -1) continue;

            var sums = _sums[index].DivideAround(cell);

            if (sums.Item1 is not null)
            {
                _sums[index] = sums.Item1;
                ReplaceCellSums(sums.Item1);
            }
            else _sums.RemoveAt(index);

            if (sums.Item2 is null) 
            { 
                if (ResizeIfNeeded()) stillNeedRemoval = false;
            }
            else
            {
                _sums.Add(sums.Item2);
                ReplaceCellSums(sums.Item2);
            }
        }

        if (stillNeedRemoval)
        {
            _cells[cell.Row, cell.Column] += null;
            _cells[cell.Row, cell.Column] -= null; 
        }

        return true;
    }

    public bool ReplaceAmount(IKakuroSum sum, int amount)
    {
        var index = IndexOf(sum);
        if (index == -1) return false;

        _sums[index].SetAmount(amount);
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

    private IArrayKakuroSum Cast(IKakuroSum sum)
    {
        if (sum is IArrayKakuroSum s) return s;

        if (sum.Orientation == Orientation.Horizontal)
            return new HorizontalKakuroSum(sum.GetStartCell(), sum.Amount, sum.Length);

        return new VerticalKakuroSum(sum.GetStartCell(), sum.Amount, sum.Length);
    }

    private int IndexOf(IKakuroSum sum)
    {
        for(int i = 0; i < _sums.Count; i++)
        {
            var s = _sums[i];
            if (IKakuroSum.AreSame(sum, s)) return i;
        }

        return -1;
    }

    private void UpdateAfterAddition(IArrayKakuroSum sum)
    {
        var fr = sum.GetFarthestRow();
        var fc = sum.GetFarthestColumn();

        if (fr >= RowCount || fc >= ColumnCount)
        {
            ResizeTo(Math.Max(RowCount, fr + 1), Math.Max(ColumnCount, fc + 1));
        }

        ReplaceCellSums(sum);
    }

    private void ReplaceCellSums(IArrayKakuroSum sum)
    {
        foreach (var cell in sum)
        {
            if (sum.Orientation == Orientation.Vertical) _cells[cell.Row, cell.Column] += sum;
            else _cells[cell.Row, cell.Column] -= sum;
        }
    }

    private bool ResizeIfNeeded()
    {
        var fr = 0;
        var fc = 0;
        foreach (var s in _sums)
        {
            fr = Math.Max(s.GetFarthestRow(), fr);
            fc = Math.Max(s.GetFarthestColumn(), fc);
        }

        fr++;
        fc++;

        if (fr != RowCount || fc != ColumnCount)
        {
            ResizeTo(fr, fc);
            return true;
        }

        return false;
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
    
    public KakuroCell(int number, IArrayKakuroSum? verticalSum, IArrayKakuroSum? horizontalSum)
    {
        Number = number;
        VerticalSum = verticalSum;
        HorizontalSum = horizontalSum;
    }

    public bool IsUsed() => VerticalSum is not null || HorizontalSum is not null;
    public bool IsRemovable() => (VerticalSum is not null && VerticalSum.Length > 1) || (HorizontalSum is not null
        && HorizontalSum.Length > 1);

    public IKakuroSum? GetSum(Orientation orientation)
    {
        return orientation == Orientation.Horizontal ? HorizontalSum : VerticalSum;
    }

    public int Number { get; }
    public IArrayKakuroSum? VerticalSum { get; }
    public IArrayKakuroSum? HorizontalSum { get; }

    public static KakuroCell operator +(KakuroCell cell, int n) => new(n, cell.VerticalSum, cell.HorizontalSum);
    public static KakuroCell operator +(KakuroCell cell, IArrayKakuroSum? v) => new(cell.Number, v, cell.HorizontalSum);
    public static KakuroCell operator -(KakuroCell cell, IArrayKakuroSum? h) => new(cell.Number, cell.VerticalSum, h);
}

public interface IArrayKakuroSum : IKakuroSum
{
    void SetAmount(int value);
    IArrayKakuroSum WithLength(int length);
    IArrayKakuroSum MoveBack(int count);
    (IArrayKakuroSum?, IArrayKakuroSum?) DivideAround(Cell cell);
}

public class VerticalKakuroSum : IArrayKakuroSum
{
    public Orientation Orientation => Orientation.Vertical;
    public int Amount { get; private set; }
    public int Length { get; }

    private readonly Cell _start;
    
    public VerticalKakuroSum(Cell start, int amount, int length)
    {
        _start = start;
        Amount = amount;
        Length = length;
    }
    
    public int GetFarthestRow() => _start.Row + Length - 1;
    public int GetFarthestColumn() => _start.Column;
    public Cell GetFarthestCell(int additionalLength) => new(_start.Row + Length - 1 + additionalLength, _start.Column);
    public Cell GetAmountCell() => new(_start.Row - 1, _start.Column);
    public Cell GetStartCell() => _start;
    public bool Contains(Cell cell)
    {
        return cell.Column == _start.Column && cell.Row >= _start.Row && cell.Row < _start.Row + Length;
    }

    public (Cell, Cell) GetPerpendicularNeighbors(int length)
    {
        var row = _start.Row + length;
        return (new Cell(row, _start.Column - 1), new Cell(row, _start.Column + 1));
    }

    public Cell this[int index] => new(_start.Row + index, _start.Column);

    public void SetAmount(int value)
    {
        Amount = value;
    }

    public IArrayKakuroSum WithLength(int length)
    {
        return new VerticalKakuroSum(_start, Amount, length);
    }

    public IArrayKakuroSum MoveBack(int count)
    {
        return new VerticalKakuroSum(new Cell(_start.Row - count, _start.Column), Amount, Length + count);
    }

    public IArrayKakuroSum WithAmount(int amount)
    {
        return new VerticalKakuroSum(_start, amount, Length);
    }

    public (IArrayKakuroSum?, IArrayKakuroSum?) DivideAround(Cell cell)
    {
        if (Length == 1) return (null, null);

        var length = cell.Row - _start.Row;
        var first = length > 0 ? WithLength(cell.Row - _start.Row) : null;
        
        length = Length - length - 1;
        return length > 0 
            ? (first, new VerticalKakuroSum(new Cell(cell.Row + 1, cell.Column), Amount, length))
            : (first, null);
    }

    public IEnumerator<Cell> GetEnumerator()
    {
        yield return _start;
        for (int i = 1; i < Length; i++)
        {
            yield return new Cell(_start.Row + i, _start.Column);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public override bool Equals(object? obj)
    {
        return obj is VerticalKakuroSum s && s._start == _start && s.Length == Length;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_start, Length, Amount, Orientation);
    }
}

public class HorizontalKakuroSum : IArrayKakuroSum
{
    public Orientation Orientation => Orientation.Horizontal;
    public int Amount { get; private set; }
    public int Length { get; }

    private readonly Cell _start;
    
    public HorizontalKakuroSum(Cell start, int amount, int length)
    {
        _start = start;
        Amount = amount;
        Length = length;
    }
    
    public int GetFarthestRow() => _start.Row;
    public int GetFarthestColumn() => _start.Column + Length - 1;
    public Cell GetFarthestCell(int additionalLength) => new(_start.Row, _start.Column + Length - 1 + additionalLength);
    public Cell GetAmountCell() => new(_start.Row, _start.Column - 1);
    public Cell GetStartCell() => _start;
    public bool Contains(Cell cell)
    {
        return cell.Row == _start.Row && cell.Column >= _start.Column && cell.Column < _start.Column + Length;
    }
    public (Cell, Cell) GetPerpendicularNeighbors(int length)
    {
        var col = _start.Column + length;
        return (new Cell(_start.Row - 1, col), new Cell(_start.Row + 1, col));
    }

    public Cell this[int index] => new(_start.Row, _start.Column + index);
    public void SetAmount(int value)
    {
        Amount = value;
    }

    public IArrayKakuroSum WithLength(int length)
    {
        return new HorizontalKakuroSum(_start, Amount, length);
    }

    public IArrayKakuroSum MoveBack(int count)
    {
        return new HorizontalKakuroSum(new Cell(_start.Row, _start.Column - count), Amount, Length + count);
    }

    public IArrayKakuroSum WithAmount(int amount)
    {
        return new HorizontalKakuroSum(_start, amount, Length);
    }
    
    public (IArrayKakuroSum?, IArrayKakuroSum?) DivideAround(Cell cell)
    {
        if (Length == 1) return (null, null);

        var length = cell.Column - _start.Column;
        var first = length > 0 ? WithLength(cell.Column - _start.Column) : null;
        
        length = Length - length - 1;
        return length > 0 
            ? (first, new HorizontalKakuroSum(new Cell(cell.Row, cell.Column + 1), Amount, length))
            : (first, null);
    }

    public IEnumerator<Cell> GetEnumerator()
    {
        yield return _start;
        for (int i = 1; i < Length; i++)
        {
            yield return new Cell(_start.Row, _start.Column + i);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override bool Equals(object? obj)
    {
        return obj is HorizontalKakuroSum s && s._start == _start && s.Length == Length;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_start, Length, Amount, Orientation);
    }
}