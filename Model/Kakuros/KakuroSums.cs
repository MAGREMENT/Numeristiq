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
    
    public IKakuroSum WithLength(int length)
    {
        return new VerticalKakuroSum(_start, Amount, length);
    }

    public IKakuroSum MoveBack(int count)
    {
        return new VerticalKakuroSum(new Cell(_start.Row - count, _start.Column), Amount, Length + count);
    }

    public IKakuroSum WithAmount(int amount)
    {
        return new VerticalKakuroSum(_start, amount, Length);
    }

    public (IKakuroSum?, IKakuroSum?) DivideAround(Cell cell)
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
    public IKakuroSum WithLength(int length)
    {
        return new HorizontalKakuroSum(_start, Amount, length);
    }

    public IKakuroSum MoveBack(int count)
    {
        return new HorizontalKakuroSum(new Cell(_start.Row, _start.Column - count), Amount, Length + count);
    }

    public IKakuroSum WithAmount(int amount)
    {
        return new HorizontalKakuroSum(_start, amount, Length);
    }
    
    public (IKakuroSum?, IKakuroSum?) DivideAround(Cell cell)
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