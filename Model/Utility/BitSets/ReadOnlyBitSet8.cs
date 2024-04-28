using System.Collections.Generic;
using System.Text;
using Model.Sudokus.Solver.Strategies;

namespace Model.Utility.BitSets;

public readonly struct ReadOnlyBitSet8
{
    public byte Bits { get; }
    public int Count { get; }

    public ReadOnlyBitSet8()
    {
        Bits = 0;
        Count = 0;
    }

    public ReadOnlyBitSet8(int i)
    {
        Bits = (byte)(1 << i);
        Count = 1;
    }

    public ReadOnlyBitSet8(params int[] numbers)
    {
        byte b = 0;
        foreach (var n in numbers)
        {
            b |= (byte)(1 << n);
        }

        Bits = b;
        Count = System.Numerics.BitOperations.PopCount(b);
    }
    
    public ReadOnlyBitSet8(IEnumerable<int> numbers)
    {
        byte b = 0;
        foreach (var n in numbers)
        {
            b |= (byte)(1 << n);
        }

        Bits = b;
        Count = System.Numerics.BitOperations.PopCount(b);
    }

    private ReadOnlyBitSet8(byte bits)
    {
        Bits = bits;
        Count = System.Numerics.BitOperations.PopCount(bits);
    }

    private ReadOnlyBitSet8(byte bits, int count)
    {
        Bits = bits;
        Count = count;
    }

    public static ReadOnlyBitSet8 Filled(int from, int to)
    {
        return new ReadOnlyBitSet8((byte)(~(0xFF << (to - from + 1)) << from));
    }

    public static ReadOnlyBitSet8 FromBits(byte bits)
    {
        return new ReadOnlyBitSet8(bits);
    }

    public bool Contains(int num)
    {
        return ((Bits >> num) & 1) > 0;
    }

    public bool ContainsAll(ReadOnlyBitSet8 set)
    {
        return (Bits | set.Bits) == Bits;
    }

    public bool ContainsAny(ReadOnlyBitSet8 set)
    {
        return (Bits & set.Bits) != 0;
    }

    public bool ContainsOnlyOne(ReadOnlyBitSet8 set)
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
        for (int i = 0; i < 8 && cursor < result.Length; i++)
        {
            if (Contains(i)) result[cursor++] = i;
        }

        return result;
    }

    public static ReadOnlyBitSet8 operator +(ReadOnlyBitSet8 set, int num)
    {
        return new ReadOnlyBitSet8((byte)(set.Bits | (1 << num)), set.Contains(num) ? set.Count : set.Count + 1);
    }

    public static ReadOnlyBitSet8 operator +(ReadOnlyBitSet8 left, ReadOnlyBitSet8 right)
    {
        return new ReadOnlyBitSet8((byte)(left.Bits | right.Bits));
    }

    public static ReadOnlyBitSet8 operator -(ReadOnlyBitSet8 set, int num)
    {
        return new ReadOnlyBitSet8((byte)(set.Bits & ~(1 << num)), set.Contains(num) ? set.Count - 1 : set.Count);
    }

    public static ReadOnlyBitSet8 operator -(ReadOnlyBitSet8 left, ReadOnlyBitSet8 right)
    {
        return new ReadOnlyBitSet8((byte)(left.Bits & ~right.Bits));
    }

    public static ReadOnlyBitSet8 operator &(ReadOnlyBitSet8 left, ReadOnlyBitSet8 right)
    {
        return new ReadOnlyBitSet8((byte)(left.Bits & right.Bits));
    }
    
    public ReadOnlyBitSet8 AndMulti(params ReadOnlyBitSet8[] sets)
    {
        var b = Bits;
        foreach (var set in sets)
        {
            b &= set.Bits;
        }

        return new ReadOnlyBitSet8(b);
    }


    public static ReadOnlyBitSet8 operator |(ReadOnlyBitSet8 left, ReadOnlyBitSet8 right)
    {
        return new ReadOnlyBitSet8((byte)(left.Bits | right.Bits));
    }

    public ReadOnlyBitSet8 OrMulti(params ReadOnlyBitSet8[] sets)
    {
        var b = Bits;
        foreach (var set in sets)
        {
            b |= set.Bits;
        }

        return new ReadOnlyBitSet8(b);
    }

    public static ReadOnlyBitSet8 operator ~(ReadOnlyBitSet8 set)
    {
        return new ReadOnlyBitSet8((byte)~set.Bits, 8 - set.Count);
    }

    public static bool operator ==(ReadOnlyBitSet8 left, ReadOnlyBitSet8 right)
    {
        return left.Bits == right.Bits;
    }
    
    public static bool operator !=(ReadOnlyBitSet8 left, ReadOnlyBitSet8 right)
    {
        return left.Bits != right.Bits;
    }
    
    public static bool operator ==(ReadOnlyBitSet8 left, BiValue right)
    {
        return left.Count == 2 && left.Contains(right.One) && left.Contains(right.Two);
    }
    
    public static bool operator !=(ReadOnlyBitSet8 left, BiValue right)
    {
        return left.Count != 2 || !left.Contains(right.One) || !left.Contains(right.Two);
    }
    
    public static bool operator ==(BiValue left, ReadOnlyBitSet8 right)
    {
        return right == left;
    }
    
    public static bool operator !=(BiValue left, ReadOnlyBitSet8 right)
    {
        return right != left;
    }

    public override bool Equals(object? obj)
    {
        return obj is ReadOnlyBitSet8 set && set.Bits == Bits;
    }

    public override int GetHashCode()
    {
        return Bits;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        for (int i = 7; i >= 0; i--)
        {
            builder.Append(Contains(i) ? '1' : '0');
        }

        return builder.ToString();
    }

    public string ToValuesString()
    {
        var builder = new StringBuilder();

        bool added = false;
        for (int i = 7; i >= 0; i--)
        {
            if (!Contains(i)) continue;
            
            if (added) builder.Append(", ");
            builder.Append(i);
            added = true;
        }
        
        return builder.ToString();
    }
}