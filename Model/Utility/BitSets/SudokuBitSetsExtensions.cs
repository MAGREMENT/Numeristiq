using System.Collections.Generic;

namespace Model.Utility.BitSets;

public static class SudokuBitSetsExtensions
{
    public static IEnumerable<int> EnumeratePossibilities(this ReadOnlyBitSet16 bitSet)
    {
        return bitSet.Enumerate(1, 9);
    }

    public static IEnumerable<int> EnumeratePositions(this ReadOnlyBitSet16 bitSet)
    {
        return bitSet.Enumerate(0, 8);
    }
    
    public static int FirstPossibility(this ReadOnlyBitSet16 bitSet)
    {
        return bitSet.First(1, 9);
    }

    public static int FirstPossibility(this ReadOnlyBitSet16 bitSet, int except)
    {
        return bitSet.First(1, 9, except);
    }

    public static int FirstPosition(this ReadOnlyBitSet16 bitSet)
    {
        return bitSet.First(0, 8);
    }

    public static int FirstPosition(this ReadOnlyBitSet16 bitSet, int except)
    {
        return bitSet.First(0, 8, except);
    }

    public static bool HasNextPossibility(this ReadOnlyBitSet16 bitSet, ref int cursor) => bitSet.HasNext(ref cursor, 9);

    public static int NextPossibility(this ReadOnlyBitSet16 bitSet, int cursor) => bitSet.Next(cursor, 9);
    
    public static bool HasNextPosition(this ReadOnlyBitSet16 bitSet, ref int cursor) => bitSet.HasNext(ref cursor, 8);

    public static int NextPosition(this ReadOnlyBitSet16 bitSet, int cursor) => bitSet.Next(cursor, 8);
}