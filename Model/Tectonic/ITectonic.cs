using System.Collections.Generic;
using Model.Helpers;
using Model.Utility;

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
    
    public IReadOnlyList<IZone> Zones { get; }
    public IZone GetZone(Cell cell);

    public IZone GetZone(int row, int col)
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

