using System.Collections.Generic;
using Model.Solver.Strategies;

namespace Model.Solver.Possibility;

public interface IReadOnlyPossibilities : IEnumerable<int>
{
    public const int Min = 1;
    public const int Max = 9;
    
    public int Count { get; }
    public int First();
    public int First(int except);
    public bool Next(ref int cursor);
    
    public Possibilities Or(IReadOnlyPossibilities possibilities);
    public Possibilities And(IReadOnlyPossibilities possibilities);
    public Possibilities Difference(IReadOnlyPossibilities possibilities);
    public Possibilities Invert();
    public bool Peek(int n);
    public bool PeekAll(IReadOnlyPossibilities poss);
    public bool PeekAny(IReadOnlyPossibilities poss);
    public bool PeekOnlyOne(IReadOnlyPossibilities poss);
    public Possibilities Copy();
    public CellState ToCellState();
    public int[] ToArray();

    public static Possibilities DefaultOr(IReadOnlyPossibilities poss1, IReadOnlyPossibilities poss2)
    {
        var result = new Possibilities();
        for (int i = Min; i <= Max; i++)
        {
            if (!poss1.Peek(i) && !poss2.Peek(i)) result.Remove(i);
        }

        return result;
    }

    public static Possibilities DefaultAnd(IReadOnlyPossibilities poss1, IReadOnlyPossibilities poss2)
    {
        var result = new Possibilities();
        for (int i = Min; i <= Max; i++)
        {
            if (!poss1.Peek(i) || !poss2.Peek(i)) result.Remove(i);
        }

        return result;
    }

    public static Possibilities DefaultDifference(IReadOnlyPossibilities poss1, IReadOnlyPossibilities poss2)
    {
        var result = poss1.Copy();

        foreach (var poss in poss2) result.Remove(poss);

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