using System.Collections.Generic;
using Model.Strategies;

namespace Model.Possibilities;

public interface IPossibilities : IEnumerable<int>
{
    public const int Min = 1;
    public const int Max = 9;
    
    public int Count { get; }
    public bool Remove(int n);
    public void RemoveAll();
    public void RemoveAll(int except);
    public void RemoveAll(params int[] except);
    public void RemoveAll(IEnumerable<int> except);
    public IPossibilities Mash(IPossibilities possibilities);
    public bool Peek(int n);
    public bool PeekAll(IPossibilities poss);
    public void Reset();
    public void Add(int n);
    public IPossibilities Copy();
    public IEnumerable<BiValue> EachBiValue();

    public static IPossibilities DefaultMash(IPossibilities poss1, IPossibilities poss2)
    {
        IPossibilities result = New();
        for (int i = Min; i <= Max; i++)
        {
            if (!poss1.Peek(i) && !poss2.Peek(i)) result.Remove(i);
        }

        return result;
    }

    public static bool DefaultPeekAll(IPossibilities poss1, IPossibilities poss2)
    {
        foreach (var possibility in poss2)
        {
            if (!poss1.Peek(possibility)) return false;
        }

        return true;
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