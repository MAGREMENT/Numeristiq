using System.Collections;
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

    public bool MergeZones(Cell c1, Cell c2);
    public bool MergeZones(IZone z1, IZone z2);
    public bool SplitZone(IEnumerable<Cell> cells);

    public void AddZone(IReadOnlyList<Cell> cells);
    public void AddZoneUnchecked(IReadOnlyList<Cell> cells);
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
    
    public bool IsFromSameZone(Cell c1, Cell c2);

    public bool IsCorrect();

    public ITectonic Copy();
}

