using System;
using System.Collections.Generic;
using Model.Utility;

namespace Model.Tectonic;

public class BlankTectonic : ITectonic
{
    public int RowCount => 0;
    public int ColumnCount => 0;
    public IReadOnlyList<Zone> Zones => Array.Empty<Zone>();

    public int this[int row, int col]
    {
        get => 0;
        set { }
    }

    public Zone GetZone(Cell cell)
    {
        return Zone.Empty();
    }

    public IEnumerable<Cell> GetNeighbors(Cell cell)
    {
        yield break;
    }

    public IEnumerable<Cell> EachCell()
    {
        yield break;
    }

    public IEnumerable<CellNumber> EachCellNumber()
    {
        yield break;
    }

    
}