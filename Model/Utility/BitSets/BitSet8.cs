using System.Collections.Generic;
using System.Text;

namespace Model.Utility.BitSets;

public class BitSet8
{
    public byte Bits { get; private set; }
    public int Count { get; private set; }

    public BitSet8()
    {
        Bits = 0;
        Count = 0;
    }

    public BitSet8(int i)
    {
        Bits = (byte)(1 << i);
        Count = 1;
    }

    public BitSet8(params int[] numbers)
    {
        byte b = 0;
        foreach (var n in numbers)
        {
            b |= (byte)(1 << n);
        }

        Bits = b;
        Count = System.Numerics.BitOperations.PopCount(b);
    }
    
    public BitSet8(IEnumerable<int> numbers)
    {
        byte b = 0;
        foreach (var n in numbers)
        {
            b |= (byte)(1 << n);
        }

        Bits = b;
        Count = System.Numerics.BitOperations.PopCount(b);
    }

    private BitSet8(byte bits)
    {
        Bits = bits;
        Count = System.Numerics.BitOperations.PopCount(bits);
    }

    private BitSet8(byte bits, int count)
    {
        Bits = bits;
        Count = count;
    }

    public static BitSet8 Filled(int from, int to)
    {
        return new BitSet8((byte)(~(0xFF << (to - from + 1)) << from));
    }

    public static BitSet8 FromBits(byte bits)
    {
        return new BitSet8(bits);
    }

    public static BitSet8 FromBitSet(ReadOnlyBitSet16 bitSet) => new(bitSet.Bits, bitSet.Count);

    public static BitSet8 FromBitSet(ReadOnlyBitSet8 bitSet) => new(bitSet.Bits, bitSet.Count);

    public bool Contains(int num)
    {
        return ((Bits >> num) & 1) > 0;
    }

    public bool ContainsAll(BitSet8 set)
    {
        return (Bits | set.Bits) == Bits;
    }

    public bool ContainsAny(BitSet8 set)
    {
        return (Bits & set.Bits) != 0;
    }

    public bool ContainsOnlyOne(BitSet8 set)
    {
        return System.Numerics.BitOperations.PopCount((byte)(Bits & set.Bits)) == 1;
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

    public void Add(int num)
    {
        if (!Contains(num)) Count++;
        Bits = (byte)(Bits | (1 << num));
    }

    public void Add(BitSet8 bitSet)
    {
        Bits = (byte)(Bits | bitSet.Bits);
        Count = System.Numerics.BitOperations.PopCount(Bits);
    }

    public void Remove(int num)
    {
        if (Contains(num)) Count--;
        Bits = (byte)(Bits & ~(1 << num));
    }

    public void Remove(BitSet8 bitSet)
    {
        Bits = (byte)(Bits & ~bitSet.Bits);
        Count = System.Numerics.BitOperations.PopCount(Bits);
    }

    public void And(BitSet8 bitSet)
    {
        Bits = (byte)(Bits & bitSet.Bits);
        Count = System.Numerics.BitOperations.PopCount(Bits);
    }
    
    public void AndMulti(params BitSet8[] sets)
    {
        foreach (var set in sets)
        {
            Bits &= set.Bits;
        }

        Count = System.Numerics.BitOperations.PopCount(Bits);
    }


    public void Or(BitSet8 bitSet)
    {
        Bits = (byte)(Bits | bitSet.Bits);
        Count = System.Numerics.BitOperations.PopCount(Bits);
    }

    public void OrMulti(params BitSet8[] sets)
    {
        foreach (var set in sets)
        {
            Bits |= set.Bits;
        }

        Count = System.Numerics.BitOperations.PopCount(Bits);
    }

    public void Reverse()
    {
        Bits = (byte)~Bits;
        Count = 8 - Count;
    }

    public override bool Equals(object? obj)
    {
        return obj is BitSet8 set && set.Bits == Bits;
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
        
        for (int i = 7; i >= 0; i--)
        {
            if (!Contains(i)) continue;
            builder.Append(i);
        }
        
        return builder.ToString();
    }
}