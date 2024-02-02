using System.Collections.Generic;
using System.Text;

namespace Model.Sudoku.Solver.BitSets;

public readonly struct ReadOnlyBitSet16
{
    public ushort Bits { get; }
    public int Count { get; }

    public ReadOnlyBitSet16()
    {
        Bits = 0;
        Count = 0;
    }
    
    private ReadOnlyBitSet16(ushort bits)
    {
        Bits = bits;
        Count = System.Numerics.BitOperations.PopCount(bits);
    }

    private ReadOnlyBitSet16(ushort bits, int count)
    {
        Bits = bits;
        Count = count;
    }

    public static ReadOnlyBitSet16 Filled(int upTo)
    {
        return new ReadOnlyBitSet16((ushort) ~(0xFFFF << upTo));
    }

    public static ReadOnlyBitSet16 FromBits(ushort bits)
    {
        return new ReadOnlyBitSet16(bits);
    }

    public bool Contains(int num)
    {
        return ((Bits >> num) & 1) > 0;
    }

    public bool ContainsAll(ReadOnlyBitSet16 set)
    {
        return (Bits | set.Bits) == Bits;
    }

    public bool ContainsAny(ReadOnlyBitSet16 set)
    {
        return (Bits & set.Bits) != 0;
    }

    public bool ContainsOnlyOne(ReadOnlyBitSet16 set)
    {
        return (this & set).Count == 1;
    }

    public IEnumerable<int> Enumerate(int from, int to)
    {
        for (; from <= to; from++)
        {
            if (Contains(from)) yield return from;
        }
    }

    public IEnumerable<int> EnumeratePossibilities()
    {
        return Enumerate(1, 9);
    }

    public IEnumerable<int> EnumeratePositions()
    {
        return Enumerate(0, 8);
    }
    
    public bool Next(ref int cursor)
    {
        cursor++;
        for (; cursor <= 9; cursor++)
        {
            if (Contains(cursor)) return true;
        }

        return false;
    }

    public int First(int from, int to)
    {
        for (; from <= to; from++)
        {
            if (Contains(from)) return from;
        }

        return -1;
    }

    public int FirstPossibility()
    {
        return First(1, 9);
    }

    public int FirstPosition()
    {
        return First(0, 8);
    }

    public int[] ToArray()
    {
        int[] result = new int[Count];
        int cursor = 0;
        for (int i = 0; i < 16 && cursor < result.Length; i++)
        {
            if (Contains(i)) result[cursor++] = i;
        }

        return result;
    }
    
    public static ReadOnlyBitSet16 operator +(ReadOnlyBitSet16 set, int num)
    {
        return new ReadOnlyBitSet16((ushort)(set.Bits | (1 << num)), set.Contains(num) ? set.Count : set.Count + 1);
    }

    public static ReadOnlyBitSet16 operator +(ReadOnlyBitSet16 left, ReadOnlyBitSet16 right)
    {
        return new ReadOnlyBitSet16((ushort)(left.Bits | right.Bits));
    }

    public static ReadOnlyBitSet16 operator -(ReadOnlyBitSet16 set, int num)
    {
        return new ReadOnlyBitSet16((ushort)(set.Bits & ~(1 << num)), set.Contains(num) ? set.Count - 1 : set.Count);
    }

    public static ReadOnlyBitSet16 operator -(ReadOnlyBitSet16 left, ReadOnlyBitSet16 right)
    {
        return new ReadOnlyBitSet16((ushort)(left.Bits & ~right.Bits));
    }

    public static ReadOnlyBitSet16 operator &(ReadOnlyBitSet16 left, ReadOnlyBitSet16 right)
    {
        return new ReadOnlyBitSet16((ushort)(left.Bits & right.Bits));
    }

    public static ReadOnlyBitSet16 operator |(ReadOnlyBitSet16 left, ReadOnlyBitSet16 right)
    {
        return new ReadOnlyBitSet16((ushort)(left.Bits | right.Bits));
    }

    public static ReadOnlyBitSet16 operator ~(ReadOnlyBitSet16 set)
    {
        return new ReadOnlyBitSet16((ushort)~set.Bits, 16 - set.Count);
    }

    public static bool operator ==(ReadOnlyBitSet16 left, ReadOnlyBitSet16 right)
    {
        return left.Bits == right.Bits;
    }
    
    public static bool operator !=(ReadOnlyBitSet16 left, ReadOnlyBitSet16 right)
    {
        return left.Bits != right.Bits;
    }

    public override bool Equals(object? obj)
    {
        return obj is ReadOnlyBitSet16 set && set.Bits == Bits;
    }

    public override int GetHashCode()
    {
        return Bits;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        for (int i = 15; i >= 0; i--)
        {
            builder.Append(Contains(i) ? '1' : '0');
        }

        return builder.ToString();
    }
}