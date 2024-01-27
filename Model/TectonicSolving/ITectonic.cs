using System.Collections;
using System.Collections.Generic;
using Global;

namespace Model.TectonicSolving;

public interface ITectonic : IReadOnlyTectonic
{ 
    public int this[int row, int col] { set; }

    public int this[Cell cell]
    {
        set => this[cell.Row, cell.Column] = value;
    }
}

public interface IReadOnlyTectonic
{
    public IReadOnlyList<Zone> Zones { get; }
    public Zone GetZone(Cell cell);

    public IEnumerable<Cell> GetNeighbors(Cell cell);

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
}