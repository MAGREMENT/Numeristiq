using System;
using System.Collections.Generic;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonic;

public class BlankTectonic : ITectonic
{
    public int RowCount => 0;
    public int ColumnCount => 0;
    public IReadOnlyList<IZone> Zones => Array.Empty<IZone>();

    public int this[int row, int col] => 0;
    public ReadOnlyBitSet16 PossibilitiesAt(int row, int col)
    {
        return new ReadOnlyBitSet16();
    }

    public IZone GetZone(Cell cell)
    {
        return EmptyZone.Instance;
    }

    public bool ShareAZone(Cell c1, Cell c2)
    {
        return false;
    }

    public IEnumerable<Cell> EachCell()
    {
        yield break;
    }

    public IEnumerable<CellNumber> EachCellNumber()
    {
        yield break;
    }


    public void Set(int n, int row, int col)
    {
        
    }
}