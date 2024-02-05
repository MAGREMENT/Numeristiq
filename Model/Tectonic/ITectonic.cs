using System;
using System.Collections;
using System.Collections.Generic;
using Model.Utility;

namespace Model.Tectonic;

public interface ITectonic : IReadOnlyTectonic
{ 
    public int this[int row, int col] { set; get; }

    public int this[Cell cell]
    {
        set => this[cell.Row, cell.Column] = value;
        get => this[cell.Row, cell.Column];
    }
}

public interface IReadOnlyTectonic
{
    public int RowCount { get; }
    public int ColumnCount { get; }
    
    public IReadOnlyList<Zone> Zones { get; }
    public Zone GetZone(Cell cell);

    public Zone GetZone(int row, int col)
    {
        return GetZone(new Cell(row, col));
    }

    public IEnumerable<Cell> GetNeighbors(Cell cell);
    public IEnumerable<Cell> GetNeighbors(int row, int col)
    {
        return GetNeighbors(new Cell(row, col));
    }

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
    private static readonly Zone _empty = new(Array.Empty<Cell>());
    public static Zone Empty() => _empty;
    

    private readonly Cell[] _cells;

    public int Count => _cells.Length;

    public Zone(Cell[] cells)
    {
        _cells = cells;
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
}