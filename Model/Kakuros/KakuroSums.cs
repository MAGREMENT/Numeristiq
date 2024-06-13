using System;
using System.Collections;
using System.Collections.Generic;
using Model.Utility;

namespace Model.Kakuros;

public class VerticalKakuroSum : IKakuroSum
{
    public Orientation Orientation => Orientation.Vertical;
    public int Amount { get; }
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
    public Cell GetAmountCell() => new(_start.Row - 1, _start.Column);
    public Cell GetStartCell() => _start;
    public bool Contains(Cell cell)
    {
        return cell.Column == _start.Column && cell.Row >= _start.Row && cell.Row < _start.Row + Length;
    }

    public Cell this[int index] => new(_start.Row + index, _start.Column);
    
    public IKakuroSum WithLength(int length)
    {
        return new VerticalKakuroSum(_start, Amount, length);
    }

    public IKakuroSum WithAmount(int amount)
    {
        return new VerticalKakuroSum(_start, amount, Length);
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

public class HorizontalKakuroSum : IKakuroSum
{
    public Orientation Orientation => Orientation.Horizontal;
    public int Amount { get; }
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
    public Cell GetAmountCell() => new(_start.Row, _start.Column - 1);
    public Cell GetStartCell() => _start;
    public bool Contains(Cell cell)
    {
        return cell.Row == _start.Row && cell.Column >= _start.Column && cell.Column < _start.Column + Length;
    }

    public Cell this[int index] => new(_start.Row, _start.Column + index);
    public IKakuroSum WithLength(int length)
    {
        return new HorizontalKakuroSum(_start, Amount, length);
    }

    public IKakuroSum WithAmount(int amount)
    {
        return new HorizontalKakuroSum(_start, amount, Length);
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