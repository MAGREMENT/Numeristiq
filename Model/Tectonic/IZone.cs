using System.Collections;
using System.Collections.Generic;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonic;

public interface IZone : IEnumerable<Cell>
{
    int Count { get; }
    
    Cell this[int index] { get; }

    bool Contains(Cell c);
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
    private readonly Cell[] _cells;

    public int Count => _cells.Length;

    public MultiZone(Cell[] cells, int columnCount)
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