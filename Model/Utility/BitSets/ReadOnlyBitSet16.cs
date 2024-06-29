using System.Collections.Generic;
using System.Text;
using Model.Sudokus.Solver.Strategies;

namespace Model.Utility.BitSets;

public readonly struct ReadOnlyBitSet16
{
    public ushort Bits { get; }
    public int Count { get; }

    public ReadOnlyBitSet16()
    {
        Bits = 0;
        Count = 0;
    }

    public ReadOnlyBitSet16(int i)
    {
        Bits = (ushort)(1 << i);
        Count = 1;
    }

    public ReadOnlyBitSet16(params int[] numbers)
    {
        ushort b = 0;
        foreach (var n in numbers)
        {
            b |= (ushort)(1 << n);
        }

        Bits = b;
        Count = System.Numerics.BitOperations.PopCount(b);
    }
    
    public ReadOnlyBitSet16(IEnumerable<int> numbers)
    {
        ushort b = 0;
        foreach (var n in numbers)
        {
            b |= (ushort)(1 << n);
        }

        Bits = b;
        Count = System.Numerics.BitOperations.PopCount(b);
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

    public static ReadOnlyBitSet16 Filled(int from, int to)
    {
        return new ReadOnlyBitSet16((ushort)(~(0xFFFF << (to - from + 1)) << from));
    }

    public static ReadOnlyBitSet16 FromBits(ushort bits)
    {
        return new ReadOnlyBitSet16(bits);
    }

    public static ReadOnlyBitSet16 FromBitSet(ReadOnlyBitSet8 bitSet) => new(bitSet.Bits, bitSet.Count);

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

    public bool HasNext(ref int cursor, int max)
    {
        cursor++;
        for (; cursor <= max; cursor++)
        {
            if (Contains(cursor)) return true;
        }

        return false;
    }

    public int Next(int cursor, int max)
    {
        cursor++;
        for (; cursor <= max; cursor++)
        {
            if (Contains(cursor)) return cursor;
        }

        return -1;
    }

    public int First(int from, int to)
    {
        for (; from <= to; from++)
        {
            if (Contains(from)) return from;
        }

        return -1;
    }

    public int First(int from, int to, int except)
    {
        for (; from <= to; from++)
        {
            if (from == except) continue;

            if (Contains(from)) return from;
        }

        return -1;
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
    
    public ReadOnlyBitSet16 AndMulti(params ReadOnlyBitSet16[] sets)
    {
        var b = Bits;
        foreach (var set in sets)
        {
            b &= set.Bits;
        }

        return new ReadOnlyBitSet16(b);
    }


    public static ReadOnlyBitSet16 operator |(ReadOnlyBitSet16 left, ReadOnlyBitSet16 right)
    {
        return new ReadOnlyBitSet16((ushort)(left.Bits | right.Bits));
    }

    public ReadOnlyBitSet16 OrMulti(params ReadOnlyBitSet16[] sets)
    {
        var b = Bits;
        foreach (var set in sets)
        {
            b |= set.Bits;
        }

        return new ReadOnlyBitSet16(b);
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
    
    public static bool operator ==(ReadOnlyBitSet16 left, BiValue right)
    {
        return left.Count == 2 && left.Contains(right.One) && left.Contains(right.Two);
    }
    
    public static bool operator !=(ReadOnlyBitSet16 left, BiValue right)
    {
        return left.Count != 2 || !left.Contains(right.One) || !left.Contains(right.Two);
    }
    
    public static bool operator ==(BiValue left, ReadOnlyBitSet16 right)
    {
        return right == left;
    }
    
    public static bool operator !=(BiValue left, ReadOnlyBitSet16 right)
    {
        return right != left;
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

    public string ToValuesString()
    {
        var builder = new StringBuilder();
        
        for (int i = 15; i >= 0; i--)
        {
            if (!Contains(i)) continue;
            builder.Append(i);
        }
        
        return builder.ToString();
    }
}