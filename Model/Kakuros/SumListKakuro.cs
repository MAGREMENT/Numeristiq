using System;
using System.Collections;
using System.Collections.Generic;
using Model.Core;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Kakuros;

public class SumListKakuro : IKakuro, INumericSolvingState
{
    private KakuroCell[,] _cells;
    private readonly List<IArrayKakuroSum> _sums;

    public int RowCount => _cells.GetLength(0);
    public int ColumnCount => _cells.GetLength(1);

    public IEnumerable<IKakuroSum> Sums => _sums;

    public SumListKakuro()
    {
        _cells = new KakuroCell[0, 0];
        _sums = new List<IArrayKakuroSum>();
    }

    public SumListKakuro(int rowCount, int colCount)
    {
        _cells = new KakuroCell[rowCount, colCount];
        _sums = new List<IArrayKakuroSum>();
    }

    private SumListKakuro(KakuroCell[,] cells, List<IArrayKakuroSum> sums)
    {
        _cells = cells;
        _sums = sums;
    }

    #region Gettings

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
    
    public IKakuroSum? FindSum(Cell amountCell)
    {
        var cell = new Cell(amountCell.Row + 1, amountCell.Column);
        if (cell is { Row: >= 0, Column: >= 0 } && cell.Row < RowCount && cell.Column < ColumnCount)
        {
            var sum = _cells[cell.Row, cell.Column].VerticalSum;
            if (sum is not null && sum.GetAmountCell() == amountCell) return sum;
        }

        if (cell is { Row: >= 0, Column: >= 0 } && cell.Row < RowCount && cell.Column < ColumnCount)
        {
            var sum = _cells[cell.Row, cell.Column].HorizontalSum;
            if (sum is not null && sum.GetAmountCell() == amountCell) return sum;
        }

        return null;
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

    public bool IsCorrect()
    {
        foreach (var sum in _sums)
        {
            var total = 0;
            var bitSet = new ReadOnlyBitSet16();
            foreach (var cell in sum)
            {
                var n = _cells[cell.Row, cell.Column].Number;
                if (n == 0) return false;

                if (bitSet.Contains(n)) return false;

                bitSet += n;
                total += n;
            }

            if (total != sum.Amount) return false;
        }

        return true;
    }

    public IKakuro Copy()
    {
        var buffer = new KakuroCell[RowCount, ColumnCount];
        Array.Copy(_cells, buffer, _cells.Length);

        return new SumListKakuro(buffer, new List<IArrayKakuroSum>(_sums));
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

    #endregion

    #region Setting

    public bool AddSum(IKakuroSum sum)
    {
        for (int i = 0; i < sum.Length; i++)
        {
            var c = sum[i];
            if (c.Row < 0 || c.Column < 0) return false;
            
            if (c.Row >= RowCount || c.Column >= ColumnCount) continue;
            
            //if (FindSum(c) is not null) return false; TODO

            var kc = _cells[c.Row, c.Column];
            if (i == 0)
            {
                if (sum.Orientation == Orientation.Horizontal)
                {
                    if (kc.HorizontalSum is not null && IKakuroSum.AreSame(kc.HorizontalSum, sum))
                    {
                        kc.HorizontalSum.SetAmount(sum.Amount);
                        return true;
                    }
                }
                else if (kc.VerticalSum is not null && IKakuroSum.AreSame(kc.VerticalSum, sum))
                {
                    kc.VerticalSum.SetAmount(sum.Amount);
                    return true;
                }
            }

            if (kc.GetSum(sum.Orientation) is not null) return false;
        }
        
        AddSumUnchecked(Cast(sum));
        return true;
    }

    public void ForceSum(IKakuroSum sum)
    {
        for (int i = 0; i < sum.Length; i++)
        {
            var c = sum[i];
            if (c.Row < 0 || c.Column < 0) return;
            
            if (c.Row >= RowCount || c.Column >= ColumnCount) continue;
            
            /*var s = FindSum(c); TODO
            if (s is not null) RemoveSum(s);*/

            var kc = _cells[c.Row, c.Column];
            if (i == 0)
            {
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
            }

            var s = kc.GetSum(sum.Orientation);
            if (s is not null) RemoveSum(s);
        }
        
        AddSumUnchecked(Cast(sum));
    }
    
    public bool RemoveSum(IKakuroSum sum)
    {
        var index = IndexOf(sum);
        if (index == -1) return false;

        _sums.RemoveAt(index);
        if (sum.Orientation == Orientation.Horizontal)
        {
            foreach (var cell in sum)
            {
                _cells[cell.Row, cell.Column] -= null;
                var s = _cells[cell.Row, cell.Column].VerticalSum;
                if(s is not null) RemoveCellFromSum(cell, s);
            }
        }
        else
        {
            foreach (var cell in sum)
            {
                _cells[cell.Row, cell.Column] |= null;
                var s = _cells[cell.Row, cell.Column].HorizontalSum;
                if(s is not null) RemoveCellFromSum(cell, s);
            }
        }

        return true;
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

        var newSum = _sums[index];
        newSum.AddToLength(lengthAdded);
        ResizeIfNeeded(newSum);
        ReplaceCellSums(newSum, newSum.Length - lengthAdded, newSum.Length); 

        var (c1, c2) = newSum.GetPerpendicularNeighbors(newSum.Length - 1);
        IArrayKakuroSum? s1 = null, s2 = null;

        if (c1.Row >= 0 && c1.Row < RowCount && c1.Column >= 0 && c1.Column < ColumnCount)
        {
            s1 = sum.Orientation == Orientation.Horizontal
                ? _cells[c1.Row, c1.Column].VerticalSum
                : _cells[c1.Row, c1.Column].HorizontalSum;
        }
        
        if (c2.Row >= 0 && c2.Row < RowCount && c2.Column >= 0 && c2.Column < ColumnCount)
        {
            s2 = sum.Orientation == Orientation.Horizontal
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
            
            s1.AddToLength(1 + additionalLength);
            ReplaceCellSums(s1, s1.Length - additionalLength, s1.Length);
        }
        else if (s2 is not null)
        {
            s2.MoveBack(1);
            ReplaceCellSums(s2, 0, 1);
        }
        else
        {
            IArrayKakuroSum s;
            var fc = newSum.GetFarthestCell(0);
            if (sum.Orientation == Orientation.Horizontal)
            {
                s = new VerticalKakuroSum(fc, 1, 1);
                _sums.Add(s);
                _cells[fc.Row, fc.Column] |= s;
            }
            else
            {
                s = new HorizontalKakuroSum(fc, 1, 1);
                _sums.Add(s);
                _cells[fc.Row, fc.Column] -= s;
            }
        }

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="sum"></param>
    /// <returns>True if the cell was totally removed</returns>
    private bool RemoveCellFromSum(Cell cell, IKakuroSum sum)
    {
        var index = IndexOf(sum);
        if (index == -1) return false;

        var sums = _sums[index].DivideAround(cell);

        if (sums.Item1 is not null)
        {
            _sums[index] = sums.Item1;
            ReplaceCellSums(sums.Item1);
        }
        else _sums.RemoveAt(index);

        if (sums.Item2 is null) 
        { 
            if (ResizeIfNeeded()) return true;
        }
        else
        {
            _sums.Add(sums.Item2);
            ReplaceCellSums(sums.Item2);
        }

        return false;
    }

    public bool RemoveCell(Cell cell) //TODO shift up or left if needed
    {
        if (!_cells[cell.Row, cell.Column].IsRemovable()) return false;

        var stillNeedRemoval = true;
        
        foreach (var sum in SumsFor(cell))
        {
            if (RemoveCellFromSum(cell, sum)) stillNeedRemoval = false;
        }

        if (stillNeedRemoval)
        {
            _cells[cell.Row, cell.Column] |= null;
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

    #endregion


    #region Private

    private void AddSumUnchecked(IArrayKakuroSum sum)
    {
        _sums.Add(sum);
        ResizeIfNeeded(sum);

        if (sum.Orientation == Orientation.Vertical)
        {
            foreach (var cell in sum)
            {
                _cells[cell.Row, cell.Column] |= sum;
                IArrayKakuroSum? leftSum = null;
                IArrayKakuroSum? rightSum = null;
                
                if (cell.Column > 0) leftSum = _cells[cell.Row, cell.Column - 1].HorizontalSum;
                if (cell.Column < ColumnCount - 1) rightSum = _cells[cell.Row, cell.Column + 1].HorizontalSum;

                if (leftSum is not null)
                {
                    if (rightSum is not null)
                    {
                        leftSum.AddToLength(rightSum.Length + 1);
                        _sums.Remove(rightSum);
                        ReplaceCellSums(leftSum);
                    }
                    else
                    {
                        leftSum.AddToLength(1);
                        _cells[cell.Row, cell.Column] -= leftSum;
                    }
                }
                else if (rightSum is not null)
                {
                    rightSum.MoveBack(1);
                    _cells[cell.Row, cell.Column] -= rightSum;
                }
                else
                {
                    var hs = new HorizontalKakuroSum(cell, 1, 1);
                    _sums.Add(hs);
                    _cells[cell.Row, cell.Column] -= hs;
                }
            }
        }
        else
        {
            foreach (var cell in sum)
            {
                _cells[cell.Row, cell.Column] -= sum;
                IArrayKakuroSum? upSum = null;
                IArrayKakuroSum? downSum = null;
                
                if (cell.Row > 0) upSum = _cells[cell.Row - 1, cell.Column].VerticalSum;
                if (cell.Row < RowCount - 1) downSum = _cells[cell.Row + 1, cell.Column].VerticalSum;

                if (upSum is not null)
                {
                    if (downSum is not null)
                    {
                        upSum.AddToLength(downSum.Length + 1);
                        _sums.Remove(downSum);
                        ReplaceCellSums(upSum);
                    }
                    else
                    {
                        upSum.AddToLength(1);
                        _cells[cell.Row, cell.Column] |= upSum;
                    }
                }
                else if (downSum is not null)
                {
                    downSum.MoveBack(1);
                    _cells[cell.Row, cell.Column] |= downSum;
                }
                else
                {
                    var vs = new VerticalKakuroSum(cell, 1, 1);
                    _sums.Add(vs);
                    _cells[cell.Row, cell.Column] |= vs;
                }
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
    
    private void ReplaceCellSums(IArrayKakuroSum sum, int start, int end)
    {
        for(int i = start; i < end; i++)
        {
            var cell = sum[i];
            if (sum.Orientation == Orientation.Vertical) _cells[cell.Row, cell.Column] |= sum;
            else _cells[cell.Row, cell.Column] -= sum;
        }
    }

    private void ReplaceCellSums(IArrayKakuroSum sum)
    {
        foreach (var cell in sum)
        {
            if (sum.Orientation == Orientation.Vertical) _cells[cell.Row, cell.Column] |= sum;
            else _cells[cell.Row, cell.Column] -= sum;
        }
    }
    
    private void ResizeIfNeeded(IKakuroSum sum)
    {
        var fr = sum.GetFarthestRow() + 1;
        var fc = sum.GetFarthestColumn() + 1;
        if (fr > RowCount || fc > ColumnCount)
        {
            ResizeTo(Math.Max(RowCount, fr), Math.Max(ColumnCount, fc));
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

    #endregion
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
    public static KakuroCell operator |(KakuroCell cell, IArrayKakuroSum? v) => new(cell.Number, v, cell.HorizontalSum);
    public static KakuroCell operator -(KakuroCell cell, IArrayKakuroSum? h) => new(cell.Number, cell.VerticalSum, h);
}

public interface IArrayKakuroSum : IKakuroSum
{
    void SetAmount(int value);
    void AddToLength(int value);
    void MoveBack(int count);
    (IArrayKakuroSum?, IArrayKakuroSum?) DivideAround(Cell cell);
}

public class VerticalKakuroSum : IArrayKakuroSum
{
    public Orientation Orientation => Orientation.Vertical;
    public int Amount { get; private set; }
    public int Length { get; private set; }

    private Cell _start;
    
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

    public void AddToLength(int value)
    {
        Length += value;
    }

    public IArrayKakuroSum WithLength(int length)
    {
        return new VerticalKakuroSum(_start, Amount, length);
    }

    public void MoveBack(int count)
    {
        _start = new Cell(_start.Row - count, _start.Column);
        Length += count;
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
        return obj is VerticalKakuroSum s && s._start == _start && s.Length == Length && s.Amount == Amount;
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
    public int Length { get; private set; }

    private Cell _start;
    
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

    public void AddToLength(int value)
    {
        Length += value;
    }

    public IArrayKakuroSum WithLength(int length)
    {
        return new HorizontalKakuroSum(_start, Amount, length);
    }

    public void MoveBack(int count)
    {
        _start = new Cell(_start.Row, _start.Column - count);
        Length += count;
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
        return obj is HorizontalKakuroSum s && s._start == _start && s.Length == Length && s.Amount == Amount;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_start, Length, Amount, Orientation);
    }
}