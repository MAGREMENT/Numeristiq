using System.Collections.Generic;

namespace Model.Core.Graphs;

public interface IGraph<TElement, TEdge> : IEnumerable<TElement> where TElement : notnull where TEdge : notnull
{
    public void Add(TElement from, TElement to, TEdge edge, LinkType type = LinkType.BiDirectional);
    public IEnumerable<TElement> Neighbors(TElement from, TEdge edge);
    public IEnumerable<TElement> Neighbors(TElement from);
    public IEnumerable<EdgeTo<TEdge, TElement>> NeighborsWithEdges(TElement from);
    public bool AreNeighbors(TElement from, TElement to, TEdge edge);
    public bool AreNeighbors(TElement from, TElement to);
    public TEdge? LinkBetween(TElement from, TElement to);
    public void Clear();
}

public enum LinkStrength
{
    None = 0, Strong = 1, Weak = 2, Any = 3
}

public enum LinkType
{
    BiDirectional, MonoDirectional
}