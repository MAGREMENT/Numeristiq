using System.Collections.Generic;

namespace Model.Sudoku.Solver.StrategiesUtility.Graphs;

public interface ILinkGraph<T> : IEnumerable<T> where T : notnull
{
    public void Add(T from, T to, LinkStrength strength, LinkType type = LinkType.BiDirectional);
    public IEnumerable<T> Neighbors(T from, LinkStrength strength);
    public IEnumerable<T> Neighbors(T from);
    public bool AreNeighbors(T from, T to, LinkStrength strength);
    public bool AreNeighbors(T from, T to);
    public void Clear();

    public static ILinkGraph<T> New()
    {
        return new DictionaryLinkGraph<T>();
    }
}

public enum LinkStrength
{
    None = 0, Strong = 1, Weak = 2, Any = 3
}