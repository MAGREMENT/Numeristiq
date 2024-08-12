using System;
using System.Collections.Generic;
using Model.Tectonics;

namespace Model.Utility.BitSets;

public static class TectonicBitSetsExtensions
{
    public static IEnumerable<int> EnumeratePossibilities(this ReadOnlyBitSet8 bitSet)
        => bitSet.Enumerate(1, IZone.MaxCount);

    public static int NextPossibility(this ReadOnlyBitSet8 bitSet, int cursor) => bitSet.Next(cursor, IZone.MaxCount);
    
    public static int NextPosition(this ReadOnlyBitSet8 bitSet, int cursor) => bitSet.Next(cursor, IZone.MaxCount - 1);

    public static IEnumerable<int> EnumeratePositions(this ReadOnlyBitSet8 bitSet, IZone zone)
        => bitSet.Enumerate(0, zone.Count - 1);
    
    public static bool HasNeighbor(this InfiniteBitMap bm, int row, int col)
    {
        var mask = (1UL << col) | (1UL << Math.Max(0, col - 1)) | (1UL << Math.Min(bm.ColumnCount - 1, col + 1));
        var min = Math.Max(0, row - 1);
        var max = Math.Min(bm.RowCount - 1, row + 1);
        for (int r = min; r <= max; r++)
        {
            var entry = r / bm.RowsPerUnit;
            var offset = (r - entry * bm.RowsPerUnit) * bm.ColumnCount;

            if ((bm.BitsAt(entry) & (mask << offset)) > 0) return true;
        }

        return false;
    }
}