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
    
    public static IEnumerable<Cell> EnumerateBoxCells(this ReadOnlyBitSet16 set, int boxNumber)
    {
        int startRow = boxNumber / 3 * 3;
        int startCol = boxNumber % 3 * 3;

        for (int i = 0; i < 9; i++)
        {
            if (set.Contains(i)) yield return new Cell(startRow + i / 3, startCol + i % 3);
        }
    }
    
    public static IEnumerable<Cell> EnumerateBoxCells(this ReadOnlyBitSet16 set, int boxRow, int boxColumn)
    {
        int startRow = boxRow / 3 * 3;
        int startCol = boxColumn % 3 * 3;

        for (int i = 0; i < 9; i++)
        {
            if (set.Contains(i)) yield return new Cell(startRow + i / 3, startCol + i % 3);
        }
    }

    public static bool AreAllInSameBox(this ReadOnlyBitSet16 set)
    {
        //111 111 000
        //111 000 111
        //000 111 111
        return set.Count is < 4 and > 0 && ((set.Bits & 0x1F8) == 0 || (set.Bits & 0x1C7) == 0 || (set.Bits & 0x3F) == 0);
    }
    
    public static ReadOnlyBitSet16 SelectBoxRow(this ReadOnlyBitSet16 set, int gridRow, int boxNumber)
    {
        var startCol = boxNumber % 3 * 3;
        var result = new ReadOnlyBitSet16();
        for (int i = 0; i < 3; i++)
        {
            if(set.Contains(gridRow * 3 + i)) result += startCol + i;
        }

        return result;
    }

    public static ReadOnlyBitSet16 SelectBoxColumn(this ReadOnlyBitSet16 set, int gridCol, int boxNumber)
    {
        var startRow = boxNumber / 3 * 3;
        var result = new ReadOnlyBitSet16();
        for (int i = 0; i < 3; i++)
        {
            if (set.Contains(i * 3 + gridCol)) result += startRow + i;
        }

        return result;
    }
    
    public static bool AreAllInSameRow(this ReadOnlyBitSet16 set)
    {
        //111 111 000
        //111 000 111
        //000 111 111
        return set.Count is < 4 and > 0 && ((set.Count & 0x1F8) == 0 || (set.Count & 0x1C7) == 0 || (set.Count & 0x3F) == 0);
    }

    public static bool AreAllInSameColumn(this ReadOnlyBitSet16 set)
    {
        //110 110 110
        //101 101 101
        //011 011 011
        return set.Count is < 4 and > 0 && ((set.Count & 0x1B6) == 0 || (set.Count & 0x16D) == 0 || (set.Count & 0xDB) == 0);
    }

    public static bool AtLeastOneInEachRows(this ReadOnlyBitSet16 set)
    {
        return System.Numerics.BitOperations.PopCount((uint)(set.Bits & 0b111)) > 0
               && System.Numerics.BitOperations.PopCount((uint)(set.Bits & 0b111000)) > 0
               && System.Numerics.BitOperations.PopCount((uint)(set.Bits & 0b111000000)) > 0;
    }
    
    public static bool AtLeastOnInEachColumns(this ReadOnlyBitSet16 set)
    {
        return System.Numerics.BitOperations.PopCount((uint)(set.Bits & 0b1001001)) > 0
               && System.Numerics.BitOperations.PopCount((uint)(set.Bits & 0b10010010)) > 0
               && System.Numerics.BitOperations.PopCount((uint)(set.Bits & 0b100100100)) > 0;
    }
}