using System.Collections.Generic;
using Model.Solver.Strategies;

namespace Model.Solver.Possibilities;

public interface IPossibilities : IReadOnlyPossibilities
{ 
    public bool Remove(int n);
    public void Remove(IPossibilities possibilities);
    public void RemoveAll();
    public void RemoveAll(int except);
    public void RemoveAll(params int[] except);
    public void RemoveAll(IEnumerable<int> except);
    public void Reset();
    public void Add(int n);
    public void Add(IReadOnlyPossibilities possibilities);

    public static void DefaultAdd(IPossibilities left, IReadOnlyPossibilities right)
    {
        foreach (var possibility in right)
        {
            left.Add(possibility);
        }
    }

    public static IPossibilities New()
    {
        return new BitPossibilities();
    }

    public static IPossibilities NewEmpty()
    {
        var buffer = new BitPossibilities();
        buffer.RemoveAll();
        return buffer;
    }
}

public interface IReadOnlyPossibilities : IEnumerable<int>
{
    public const int Min = 1;
    public const int Max = 9;
    
    public int Count { get; }
    public int GetFirst();
    public IPossibilities Or(IReadOnlyPossibilities possibilities);
    public int OrCount(IReadOnlyPossibilities possibilities);
    public IPossibilities And(IReadOnlyPossibilities possibilities);
    public bool Peek(int n);
    public bool PeekAll(IReadOnlyPossibilities poss);
    public bool PeekAny(IReadOnlyPossibilities poss);
    public bool PeekOnlyOne(IReadOnlyPossibilities poss);
    public IPossibilities Copy();
    public IEnumerable<BiValue> EachBiValue();
    
    public static IPossibilities DefaultOr(IReadOnlyPossibilities poss1, IReadOnlyPossibilities poss2)
    {
        var result = IPossibilities.New();
        for (int i = Min; i <= Max; i++)
        {
            if (!poss1.Peek(i) && !poss2.Peek(i)) result.Remove(i);
        }

        return result;
    }

    public static int DefaultOrCount(IReadOnlyPossibilities poss1, IReadOnlyPossibilities poss2)
    {
        var result = 0;
        for (int i = Min; i <= Max; i++)
        {
            if (poss1.Peek(i) || poss2.Peek(i)) result++;
        }

        return result;
    }

    public static IPossibilities DefaultAnd(IReadOnlyPossibilities poss1, IReadOnlyPossibilities poss2)
    {
        var result = IPossibilities.New();
        for (int i = Min; i <= Max; i++)
        {
            if (!poss1.Peek(i) || !poss2.Peek(i)) result.Remove(i);
        }

        return result;
    }

    public static bool DefaultPeekAll(IReadOnlyPossibilities poss1, IReadOnlyPossibilities poss2)
    {
        foreach (var possibility in poss2)
        {
            if (!poss1.Peek(possibility)) return false;
        }

        return true;
    }

    public static bool DefaultPeekAny(IReadOnlyPossibilities poss1, IReadOnlyPossibilities poss2)
    {
        foreach (var possibility in poss2)
        {
            if (poss1.Peek(possibility)) return true;
        }

        return false;
    }
    
    public static bool DefaultPeekOnlyOne(IReadOnlyPossibilities poss1, IReadOnlyPossibilities poss2)
    {
        bool foundOne = false;
        foreach (var possibility in poss2)
        {
            if (poss1.Peek(possibility))
            {
                if (foundOne) return false;
                
                foundOne = true;
            }
        }

        return true;
    }
}