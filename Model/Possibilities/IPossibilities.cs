using System.Collections.Generic;
using Model.Strategies;

namespace Model.Possibilities;

public interface IPossibilities : IReadOnlyPossibilities //TODO look into structs
{
    public const int Min = 1;
    public const int Max = 9;
    
    public bool Remove(int n);
    public void RemoveAll();
    public void RemoveAll(int except);
    public void RemoveAll(params int[] except);
    public void RemoveAll(IEnumerable<int> except);
    public void Reset();
    public void Add(int n);

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

public interface IReadOnlyPossibilities : IEnumerable<int>
{
    public int Count { get; }
    public int GetFirst();
    public IPossibilities Mash(IPossibilities possibilities);
    public bool Peek(int n);
    public bool PeekAll(IPossibilities poss);
    public IPossibilities Copy();
    public IEnumerable<BiValue> EachBiValue();
}