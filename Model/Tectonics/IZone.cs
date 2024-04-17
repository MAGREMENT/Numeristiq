using System.Collections;
using System.Collections.Generic;
using Model.Utility;
using Model.Utility.BitSets;
using Model.Utility.Collections;

namespace Model.Tectonics;

public interface IZone : IContainingEnumerable<Cell>
{
    int Count { get; }
    
    Cell this[int index] { get; }
}

public class EmptyZone : IZone
{
    public static EmptyZone Instance { get; } = new();
    
    public IEnumerator<Cell> GetEnumerator()
    {
        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => 0;

    public Cell this[int index] => new();

    public bool Contains(Cell c) => false;
}

public class MultiZone : IZone
{
    private readonly int _columnCount;
    private readonly InfiniteBitSet _id;
    private readonly IReadOnlyList<Cell> _cells;

    public int Count => _cells.Count;

    public MultiZone(IReadOnlyList<Cell> cells, int columnCount)
    {
        _id = new InfiniteBitSet();
        _cells = cells;
        _columnCount = columnCount; 

        foreach (var cell in _cells)
        {
            _id.Set(cell.Row * columnCount + cell.Column);
        }
    }

    public MultiZone(Cell cell, int columnCount) : this(new[] {cell}, columnCount)
    {
        
    }

    public IEnumerator<Cell> GetEnumerator()
    {
        foreach (var cell in _cells)
        {
            yield return cell;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Cell this[int index] => _cells[index];

    public bool Contains(Cell c)
    {
        return _id.IsSet(c.Row * _columnCount + c.Column);
    }

    public override bool Equals(object? obj)
    {
        if (obj is MultiZone mz)
        {
            return mz._id == _id;
        }

        if (obj is IZone z)
        {
            if (z.Count != Count) return false;

            foreach (var cell in z)
            {
                if (!Contains(cell)) return false;
            }

            return true;
        }
        
        return false;
    }

    public override int GetHashCode()
    {
        return _id.GetHashCode();
    }
}

public class SoloZone : IZone
{
    public Cell Cell { get; }

    public SoloZone(Cell cell)
    {
        Cell = cell;
    }

    public SoloZone(int row, int col)
    {
        Cell = new Cell(row, col);
    }

    public IEnumerator<Cell> GetEnumerator()
    {
        yield return Cell;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => 1;

    public Cell this[int index] => Cell;

    public bool Contains(Cell c) => c == Cell;
}