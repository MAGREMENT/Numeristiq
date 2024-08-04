using System.Collections.Generic;
using System.Text;

namespace Model.Utility.BitSets;

public class BitSet16
{
    public ushort Bits { get; private set; }
    public int Count { get; private set; }

    public BitSet16()
    {
        Bits = 0;
        Count = 0;
    }

    public BitSet16(int i)
    {
        Bits = (ushort)(1 << i);
        Count = 1;
    }

    public BitSet16(params int[] numbers)
    {
        ushort b = 0;
        foreach (var n in numbers)
        {
            b |= (ushort)(1 << n);
        }

        Bits = b;
        Count = System.Numerics.BitOperations.PopCount(b);
    }
    
    public BitSet16(IEnumerable<int> numbers)
    {
        ushort b = 0;
        foreach (var n in numbers)
        {
            b |= (ushort)(1 << n);
        }

        Bits = b;
        Count = System.Numerics.BitOperations.PopCount(b);
    }

    private BitSet16(ushort bits)
    {
        Bits = bits;
        Count = System.Numerics.BitOperations.PopCount(bits);
    }

    private BitSet16(ushort bits, int count)
    {
        Bits = bits;
        Count = count;
    }

    public static BitSet16 Filled(int from, int to)
    {
        return new BitSet16((ushort)(~(0xFFFF << (to - from + 1)) << from));
    }

    public static BitSet16 FromBits(ushort bits)
    {
        return new BitSet16(bits);
    }

    public static BitSet16 FromBitSet(ReadOnlyBitSet16 bitSet16) => new(bitSet16.Bits, bitSet16.Count);

    public static BitSet16 FromBitSet(ReadOnlyBitSet8 bitSet) => new(bitSet.Bits, bitSet.Count);

    public bool Contains(int num)
    {
        return ((Bits >> num) & 1) > 0;
    }

    public bool ContainsAll(BitSet16 set)
    {
        return (Bits | set.Bits) == Bits;
    }

    public bool ContainsAny(BitSet16 set)
    {
        return (Bits & set.Bits) != 0;
    }

    public bool ContainsOnlyOne(BitSet16 set)
    {
        return System.Numerics.BitOperations.PopCount((ushort)(Bits & set.Bits)) == 1;
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

    public void Add(int num)
    {
        if (!Contains(num)) Count++;
        Bits = (ushort)(Bits | (1 << num));
    }

    public void Add(BitSet16 bitSet)
    {
        Bits = (ushort)(Bits | bitSet.Bits);
        Count = System.Numerics.BitOperations.PopCount(Bits);
    }

    public void Remove(int num)
    {
        if (Contains(num)) Count--;
        Bits = (ushort)(Bits & ~(1 << num));
    }

    public void Remove(BitSet16 bitSet)
    {
        Bits = (ushort)(Bits & ~bitSet.Bits);
        Count = System.Numerics.BitOperations.PopCount(Bits);
    }

    public void And(BitSet16 bitSet)
    {
        Bits = (ushort)(Bits & bitSet.Bits);
        Count = System.Numerics.BitOperations.PopCount(Bits);
    }
    
    public void AndMulti(params BitSet16[] sets)
    {
        foreach (var set in sets)
        {
            Bits &= set.Bits;
        }

        Count = System.Numerics.BitOperations.PopCount(Bits);
    }


    public void Or(BitSet16 bitSet)
    {
        Bits = (ushort)(Bits | bitSet.Bits);
        Count = System.Numerics.BitOperations.PopCount(Bits);
    }

    public void OrMulti(params BitSet16[] sets)
    {
        foreach (var set in sets)
        {
            Bits |= set.Bits;
        }

        Count = System.Numerics.BitOperations.PopCount(Bits);
    }

    public void Reverse()
    {
        Bits = (ushort)~Bits;
        Count = 16 - Count;
    }

    public override bool Equals(object? obj)
    {
        return obj is BitSet16 set && set.Bits == Bits;
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