using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Model.Solver.Possibility;

public class Possibilities : IReadOnlyPossibilities
{
    private int _possibilities;
    public int Count { private set; get; }

    private Possibilities(int possibilities, int count)
    {
        _possibilities = possibilities;
        Count = count;
    }

    public Possibilities()
    {
        _possibilities = 0x3FE;
        Count = 9;
    }

    public static Possibilities FromBits(int bits)
    {
        return new Possibilities(bits, System.Numerics.BitOperations.PopCount((uint)bits));
    }
    
    public static Possibilities FromEnumerable(IEnumerable<int> enumerable)
    {
        var result = NewEmpty();
        foreach (var n in enumerable)
        {
            result.Add(n);
        }

        return result;
    }

    public static Possibilities NewEmpty()
    {
        return new Possibilities(0, 0);
    }
    
    public bool Remove(int number)
    {
        bool old = Peek(number);
        _possibilities &= ~(1 << number);
        if (old) Count--;
        return old;
    }

    public void Remove(Possibilities possibilities)
    {
        _possibilities &= ~possibilities._possibilities;
        Count = System.Numerics.BitOperations.PopCount((uint)_possibilities);
    }

    public void RemoveAll()
    {
        _possibilities = 0;
        Count = 0;
    }

    public int First(int except)
    {
        for (int i = 1; i <= 9; i++)
        {
            if (i == except) continue;

            if (Peek(i)) return i;
        }

        return 0;
    }

    public bool Next(ref int cursor)
    {
        cursor++;
        for (; cursor <= 9; cursor++)
        {
            if (Peek(cursor)) return true;
        }

        return false;
    }

    public Possibilities Or(IReadOnlyPossibilities possibilities)
    {
        if (possibilities is Possibilities bp)
        { 
            int or = _possibilities | bp._possibilities;
            return new Possibilities(or, System.Numerics.BitOperations.PopCount((uint) or));
        }
        return IReadOnlyPossibilities.DefaultOr(this, possibilities);
    }

    public Possibilities Or(params IReadOnlyPossibilities[] possibilities)
    {
        var or = _possibilities;
        foreach (var p in possibilities)
        {
            or |= ((Possibilities)p)._possibilities;
        }
        
        return new Possibilities(or, System.Numerics.BitOperations.PopCount((uint) or));
    }

    public Possibilities And(IReadOnlyPossibilities possibilities)
    {
        if (possibilities is Possibilities bp)
        { 
            int and = _possibilities & bp._possibilities;
            return new Possibilities(and, System.Numerics.BitOperations.PopCount((uint) and));
        }
        
        return IReadOnlyPossibilities.DefaultAnd(this, possibilities);
    }

    public Possibilities Difference(IReadOnlyPossibilities possibilities)
    {
        if (possibilities is Possibilities p)
        {
            var diff = _possibilities & ~p._possibilities;
            return new Possibilities(diff, System.Numerics.BitOperations.PopCount((uint) diff));
        }

        return IReadOnlyPossibilities.DefaultDifference(this, possibilities);
    }

    public Possibilities Invert()
    {
        return new Possibilities(~_possibilities & 0x3FE, 9 - Count);
    }

    public bool Peek(int number)
    {
        return ((_possibilities >> number) & 1) > 0;
    }

    public bool PeekAll(IReadOnlyPossibilities poss)
    {
        if (poss is Possibilities bp) return (_possibilities | bp._possibilities) == _possibilities;
        return IReadOnlyPossibilities.DefaultPeekAll(this, poss);
    }

    public bool PeekAny(IReadOnlyPossibilities poss)
    {
        if (poss is Possibilities bp)
            return System.Numerics.BitOperations.PopCount((uint)(bp._possibilities & _possibilities)) > 0;
        return IReadOnlyPossibilities.DefaultPeekAny(this, poss);
    }

    public bool PeekOnlyOne(IReadOnlyPossibilities poss)
    {
        if (poss is Possibilities bp)
            return System.Numerics.BitOperations.PopCount((uint)(bp._possibilities & _possibilities)) == 1;
        return IReadOnlyPossibilities.DefaultPeekOnlyOne(this, poss);
    }

    public void Reset()
    {
        _possibilities = 0x3FE;
        Count = 9;
    }

    public void Add(int n)
    {
        if (Peek(n)) return;
        _possibilities |= 1 << n;
        Count++;
    }

    public void Add(IReadOnlyPossibilities possibilities)
    {
        if (possibilities is Possibilities bp)
        {
            _possibilities |= bp._possibilities;
            Count = System.Numerics.BitOperations.PopCount((uint)_possibilities);
        }
    }

    public int First()
    {
        for (int i = 1; i <= 9; i++)
        {
            if (Peek(i)) return i;
        }
        
        return 0;
    }

    public Possibilities Copy()
    {
        return new Possibilities(_possibilities, Count);
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

    public override bool Equals(object? obj)
    {
        if (obj is not Possibilities p) return false;
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

    public string ToSlimString()
    {
        var builder = new StringBuilder();

        foreach (var poss in this)
        {
            builder.Append(poss.ToString());
        }
        
        return builder.ToString();
    }

    public int[] ToArray()
    {
        var result = new int[Count];
        
        var cursor = 0;
        foreach (var n in this)
        {
            result[cursor++] = n;
        }

        return result;
    }
    
    public IEnumerator<int> GetEnumerator()
    {
        for (int i = 1; i <= 9; i++)
        {
            if (Peek(i)) yield return i;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}