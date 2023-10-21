using System.Collections;
using System.Collections.Generic;
using Model.Solver.Strategies;

namespace Model.Solver.Possibilities;

public class BitPossibilities : IPossibilities
{
    private int _possibilities;
    public int Count { private set; get; }

    private BitPossibilities(int possibilities, int count)
    {
        _possibilities = possibilities;
        Count = count;
    }

    public BitPossibilities()
    {
        _possibilities = 0x1FF;
        Count = 9;
    }

    public static BitPossibilities FromBits(int bits)
    {
        return new BitPossibilities(bits, System.Numerics.BitOperations.PopCount((uint)bits));
    }
    
    public bool Remove(int number)
    {
        bool old = Peek(number);
        _possibilities &= ~(1 << (number - 1));
        if (old) Count--;
        return old;
    }

    public void Remove(IPossibilities possibilities)
    {
        if (possibilities is not BitPossibilities bp)
        {
            IPossibilities.DefaultRemove(this, possibilities);
            return;
        }

        _possibilities &= ~bp._possibilities;
        Count = System.Numerics.BitOperations.PopCount((uint)_possibilities);
    }

    public void RemoveAll()
    {
        _possibilities = 0;
        Count = 0;
    }

    public void RemoveAll(int except)
    {
        _possibilities &= 1 << (except - 1);
        Count = 1;
    }

    public void RemoveAll(params int[] except)
    {
        RemoveAll();
        foreach (var num in except)
        {
            _possibilities |= 1 << (num - 1);
            Count++;
        }
    }

    public void RemoveAll(IEnumerable<int> except)
    {
        RemoveAll();
        foreach (var num in except)
        {
            _possibilities |= 1 << (num - 1);
            Count++;
        }
    }

    public IPossibilities Or(IReadOnlyPossibilities possibilities)
    {
        if (possibilities is BitPossibilities bp)
        { 
            int or = _possibilities | bp._possibilities;
            return new BitPossibilities(or, System.Numerics.BitOperations.PopCount((uint) or));
        }
        return IPossibilities.DefaultOr(this, possibilities);
    }

    public IPossibilities And(IReadOnlyPossibilities possibilities)
    {
        if (possibilities is BitPossibilities bp)
        { 
            int and = _possibilities & bp._possibilities;
            return new BitPossibilities(and, System.Numerics.BitOperations.PopCount((uint) and));
        }
        return IPossibilities.DefaultOr(this, possibilities);
    }

    public IPossibilities Invert()
    {
        return new BitPossibilities(~_possibilities & 0x1FF, 9 - Count);
    }

    public bool Peek(int number)
    {
        return ((_possibilities >> (number - 1)) & 1) > 0;
    }

    public bool PeekAll(IReadOnlyPossibilities poss)
    {
        if (poss is BitPossibilities bp) return (_possibilities | bp._possibilities) == _possibilities;
        return IPossibilities.DefaultPeekAll(this, poss);
    }

    public bool PeekAny(IReadOnlyPossibilities poss)
    {
        if (poss is BitPossibilities bp)
            return System.Numerics.BitOperations.PopCount((uint)(bp._possibilities & _possibilities)) > 0;
        return IPossibilities.DefaultPeekAny(this, poss);
    }

    public bool PeekOnlyOne(IReadOnlyPossibilities poss)
    {
        if (poss is BitPossibilities bp)
            return System.Numerics.BitOperations.PopCount((uint)(bp._possibilities & _possibilities)) == 1;
        return IPossibilities.DefaultPeekOnlyOne(this, poss);
    }

    public void Reset()
    {
        _possibilities = 0x1FF;
        Count = 9;
    }

    public void Add(int n)
    {
        if (Peek(n)) return;
        _possibilities |= 1 << (n - 1);
        Count++;
    }

    public void Add(IReadOnlyPossibilities possibilities)
    {
        if (possibilities is BitPossibilities bp)
        {
            _possibilities |= bp._possibilities;
            Count = System.Numerics.BitOperations.PopCount((uint)_possibilities);
            return;
        }

        IPossibilities.DefaultAdd(this, possibilities);
    }

    public int First()
    {
        for (int i = 0; i < 9; i++)
        {
            if (((_possibilities >> i) & 1) > 0) return i + 1;
        }
        
        return 0;
    }

    public IPossibilities Copy()
    {
        return new BitPossibilities(_possibilities, Count);
    }

    public IEnumerable<BiValue> EachBiValue()
    {
        for (int i = 1; i <= 9; i++)
        {
            if (!Peek(i)) continue;
            for (int j = i + 1; j <= 9; j++)
            {
                if (Peek(j)) yield return new BiValue(i, j);
            }
        }
    }

    public CellState ToCellState()
    {
        return CellState.FromBits((short)_possibilities);
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return _possibilities;
    }

    public IEnumerator<int> GetEnumerator()
    {
        for (int i = 0; i < 9; i++)
        {
            if (((_possibilities >> i) & 1) > 0)
            {
                yield return i + 1;
            }
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BitPossibilities p) return false;
        return p._possibilities == _possibilities;
    }

    public override string ToString()
    {
        string result = "(";
        for (int i = 1; i <= 9; i++)
        {
            if (Peek(i)) result += i + ", ";
        }

        if (result.Length != 1) result = result[..^2];
        return result + ")";
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}