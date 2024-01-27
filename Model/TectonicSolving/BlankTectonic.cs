using System;
using System.Collections.Generic;
using Global;

namespace Model.TectonicSolving;

public class BlankTectonic : ITectonic
{
    public IReadOnlyList<Zone> Zones => Array.Empty<Zone>();

    public int this[int row, int col]
    {
        set { }
    }

    public Zone GetZone(Cell cell)
    {
        return default!;
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