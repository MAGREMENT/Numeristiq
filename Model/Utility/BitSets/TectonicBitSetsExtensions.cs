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
}