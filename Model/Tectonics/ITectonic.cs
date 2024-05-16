using System.Collections.Generic;
using Model.Helpers;
using Model.Utility;

namespace Model.Tectonics;

public interface ITectonic : IReadOnlyTectonic
{ 
    public int this[Cell cell]
    {
        set => this[cell.Row, cell.Column] = value;
        get => this[cell.Row, cell.Column];
    }
    
    public new int this[int row, int col] { get; set; }

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
    public int GetZoneNumber(IZone zone);

    public IZone GetZone(int row, int col)
    {
        return GetZone(new Cell(row, col));
    }
    
    public bool IsFromSameZone(Cell c1, Cell c2);

    public bool IsCorrect();
    public bool IsComplete();

    public ITectonic Copy();
    public ITectonic CopyNumberLess();
    public ITectonic Transfer(int rowCount, int columnCount);
}

