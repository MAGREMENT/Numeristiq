using System;
using System.Collections;
using System.Collections.Generic;
using Model.Helpers;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonic;

public interface ITectonic : IReadOnlyTectonic
{
    public void Set(int n, int row, int col);

    public int this[Cell cell]
    {
        set => Set(value, cell.Row, cell.Column);
        get => this[cell.Row, cell.Column];
    }
}

public interface IReadOnlyTectonic : ISolvingState
{
    public int RowCount { get; }
    public int ColumnCount { get; }
    
    public IReadOnlyList<Zone> Zones { get; }
    public Zone GetZone(Cell cell);

    public Zone GetZone(int row, int col)
    {
        return GetZone(new Cell(row, col));
    }
    
    public bool ShareAZone(Cell c1, Cell c2);

    public IEnumerable<Cell> EachCell();
    public IEnumerable<CellNumber> EachCellNumber();
}

public readonly struct CellNumber 
{
    public CellNumber(Cell cell, int number)
    {
        Cell = cell;
        Number = number;
    }

    public Cell Cell { get; }
    public int Number { get; }

    public bool IsSet()
    {
        return Number != 0;
    }
}

public class Zone : IEnumerable<Cell>
{
    private static readonly Zone _empty = new(Array.Empty<Cell>(), 0);
    public static Zone Empty() => _empty;

    private readonly int _columnCount;
    private readonly InfiniteBitSet _id;
    private readonly Cell[] _cells;

    public int Count => _cells.Length;

    public Zone(Cell[] cells, int columnCount)
    {
        _id = new InfiniteBitSet();
        _cells = cells;
        _columnCount = columnCount; 

        foreach (var cell in _cells)
        {
            _id.Set(cell.Row * columnCount + cell.Column);
        }
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
        return obj is Zone z && z._id.Equals(_id);
    }

    public override int GetHashCode()
    {
        return _id.GetHashCode();
    }
}