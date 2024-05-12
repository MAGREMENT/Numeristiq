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
}