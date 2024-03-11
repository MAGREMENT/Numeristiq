using System.Collections.Generic;

namespace Model.Utility.BitSets;

public static class BitSetHelper
{
    public static IEnumerable<Cell> EnumerateBoxCells(ReadOnlyBitSet16 set, int boxNumber)
    {
        int startRow = boxNumber / 3 * 3;
        int startCol = boxNumber % 3 * 3;

        for (int i = 0; i < 9; i++)
        {
            if (set.Contains(i)) yield return new Cell(startRow + i / 3, startCol + i % 3);
        }
    }
    
    public static IEnumerable<Cell> EnumerateBoxCells(ReadOnlyBitSet16 set, int boxRow, int boxColumn)
    {
        int startRow = boxRow / 3 * 3;
        int startCol = boxColumn % 3 * 3;

        for (int i = 0; i < 9; i++)
        {
            if (set.Contains(i)) yield return new Cell(startRow + i / 3, startCol + i % 3);
        }
    }

    public static bool AreAllInSameBox(ReadOnlyBitSet16 set)
    {
        //111 111 000
        //111 000 111
        //000 111 111
        return set.Count is < 4 and > 0 && ((set.Bits & 0x1F8) == 0 || (set.Bits & 0x1C7) == 0 || (set.Bits & 0x3F) == 0);
    }
    
    public static ReadOnlyBitSet16 BoxToRow(ReadOnlyBitSet16 set, int boxNumber)
    {
        return new ReadOnlyBitSet16(); //TODO
    }

    public static ReadOnlyBitSet16 BoxToColumn(int gridCol)
    {
        return new ReadOnlyBitSet16(); //TODO
    }
    
    public static bool AreAllInSameRow(ReadOnlyBitSet16 set)
    {
        //111 111 000
        //111 000 111
        //000 111 111
        return set.Count is < 4 and > 0 && ((set.Count & 0x1F8) == 0 || (set.Count & 0x1C7) == 0 || (set.Count & 0x3F) == 0);
    }

    public static bool AreAllInSameColumn(ReadOnlyBitSet16 set)
    {
        //110 110 110
        //101 101 101
        //011 011 011
        return set.Count is < 4 and > 0 && ((set.Count & 0x1B6) == 0 || (set.Count & 0x16D) == 0 || (set.Count & 0xDB) == 0);
    }

    public static bool AtLeastOneInEachRows(ReadOnlyBitSet16 set)
    {
        return System.Numerics.BitOperations.PopCount((uint)(set.Bits & 0b111)) > 0
               && System.Numerics.BitOperations.PopCount((uint)(set.Bits & 0b111000)) > 0
               && System.Numerics.BitOperations.PopCount((uint)(set.Bits & 0b111000000)) > 0;
    }
    
    public static bool AtLeastOnInEachColumns(ReadOnlyBitSet16 set)
    {
        return System.Numerics.BitOperations.PopCount((uint)(set.Bits & 0b1001001)) > 0
               && System.Numerics.BitOperations.PopCount((uint)(set.Bits & 0b10010010)) > 0
               && System.Numerics.BitOperations.PopCount((uint)(set.Bits & 0b100100100)) > 0;
    }

    public static IEnumerable<ReadOnlyBitSet16> EveryDiagonalPattern(ReadOnlyBitSet16 set)
    {
        /*for (int i = 0; i < 3; i++)
        {
            if (!Contains(i, 0)) continue;
            for (int j = 0; j < 3; j++)
            {
                if (j == i || !Contains(j, 1)) continue;
                for (int k = 0; k < 3; k++)
                {
                    if (k == i || k == j || !Contains(k, 2)) continue;

                    var mgp = new MiniGridPositions(_startRow / 3, _startCol / 3);
                    mgp.Add(i, 0);
                    mgp.Add(j, 1);
                    mgp.Add(k, 2);

                    yield return mgp;
                }
            }
        }*/

        yield break; //TODO
    }
}