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

    public bool IsFromSameZone(Cell c1, Cell c2)
    {
        return false;
    }

    public bool IsCorrect()
    {
        return false;
    }

    public void Set(int n, int row, int col)
    {
        
    }

    public void MergeZones(Cell c1, Cell c2)
    {
        
    }

    public void AddZone(IReadOnlyList<Cell> cells)
    {
        
    }

    public void AddZoneUnchecked(IReadOnlyList<Cell> cells)
    {
        
    }
}