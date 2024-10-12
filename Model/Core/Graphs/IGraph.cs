using System;
using System.Collections.Generic;

namespace Model.Core.Graphs;

public interface IClearable
{
    public void Clear();
}

public interface IGraph<TElement, TEdge> : IEnumerable<TElement>, IClearable 
    where TElement : notnull where TEdge : notnull
{
    public void Add(TElement from, TElement to, TEdge edge, LinkType type = LinkType.BiDirectional);
    public IEnumerable<TElement> Neighbors(TElement from, TEdge edge);
    public IEnumerable<TElement> Neighbors(TElement from);
    public IEnumerable<EdgeTo<TEdge, TElement>> NeighborsWithEdges(TElement from);
    public bool AreNeighbors(TElement from, TElement to, TEdge edge);
    public bool AreNeighbors(TElement from, TElement to);
    public TEdge? LinkBetween(TElement from, TElement to);
}

public interface IConditionalGraph<TElement, TEdge, TValue> : IGraph<TElement, TEdge>
    where TElement : notnull where TEdge : notnull
{
    public IValueCollection<TElement, TValue>? Values { set; }
    
    public void Add(ICondition<TElement, TValue> condition, TElement from, TElement to, TEdge edge,
        LinkType type = LinkType.BiDirectional);
}

public interface ICondition<out TElement, TValue> where TElement : notnull
{
    public bool IsMet(IValueCollection<TElement, TValue> values);
}

public interface IValueCollection<in TElement, TValue>
{
    bool TryGetElementValue(TElement element, out TValue value);
}

public enum LinkStrength
{
    None = 0, Strong = 1, Weak = 2, Any = 3
}

public static class LinkStrengthExtensions
{
    public static char ToChar(this LinkStrength ls)
    {
        return ls switch
        {
            LinkStrength.Strong => '=',
            LinkStrength.Weak => '-',
            LinkStrength.None => '|',
            LinkStrength.Any => '?',
            _ => throw new ArgumentOutOfRangeException(nameof(ls), ls, null)
        };
    }
}

public enum LinkType
{
    BiDirectional, MonoDirectional
}