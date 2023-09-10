using System.Collections;
using System.Collections.Generic;
using Model.Strategies;

namespace Model.Possibilities;

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
    
    public bool Remove(int number)
    {
        bool old = Peek(number);
        _possibilities &= ~(1 << (number - 1));
        if (old) Count--;
        return old;
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

    public IPossibilities Mash(IPossibilities possibilities)
    {
        if (possibilities is BitPossibilities bp)
        { 
            int mashed = _possibilities | bp._possibilities;
            return new BitPossibilities(mashed, System.Numerics.BitOperations.PopCount((uint) mashed));
        }
        return IPossibilities.DefaultMash(this, possibilities);
    }

    public bool Peek(int number)
    {
        return ((_possibilities >> (number - 1)) & 1) > 0;
    }

    public bool PeekAll(IPossibilities poss)
    {
        if (poss is BitPossibilities bp) return (_possibilities | bp._possibilities) == _possibilities;
        return IPossibilities.DefaultPeekAll(this, poss);
    }

    public bool PeekAny(IPossibilities poss)
    {
        return IPossibilities.DefaultPeekAny(this, poss);
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

    public int GetFirst()
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
        string result = "[";
        for (int i = 1; i <= 9; i++)
        {
            if (Peek(i)) result += i + ", ";
        }

        if (result.Length != 1) result = result[..^2];
        return result + "]";
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}