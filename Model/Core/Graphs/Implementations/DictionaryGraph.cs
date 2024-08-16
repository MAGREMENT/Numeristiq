using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.Utility.Collections;

namespace Model.Core.Graphs.Implementations;

public abstract class DictionaryGraph<TElement, TEdge> : IGraph<TElement, TEdge> where TElement : notnull where TEdge : notnull
{
    private readonly Dictionary<TElement, IContainingCollection<EdgeTo<TEdge, TElement>>> _edges = new();

    protected abstract IContainingCollection<EdgeTo<TEdge, TElement>> CreateCollection();
    
    public void Add(TElement from, TElement to, TEdge edge, LinkType type = LinkType.BiDirectional)
    {
        if (!_edges.TryGetValue(from, out var hs))
        {
            hs = CreateCollection();
            _edges[from] = hs;
        }

        hs.Add(new EdgeTo<TEdge, TElement>(edge, to));
        
        if (type != LinkType.BiDirectional) return;
        
        if (!_edges.TryGetValue(to, out hs))
        {
            hs = CreateCollection();
            _edges[to] = hs;
        }
        hs.Add(new EdgeTo<TEdge, TElement>(edge, from));
    }

    public IEnumerable<TElement> Neighbors(TElement from, TEdge edge)
    {
        foreach (var edgeTo in NeighborsWithEdges(from))
        {
            if (edgeTo.Edge.Equals(edge)) yield return edgeTo.To;
        }
    }

    public IEnumerable<TElement> Neighbors(TElement from)
    {
        foreach (var edgeTo in NeighborsWithEdges(from))
        {
            yield return edgeTo.To;
        }
    }

    public IEnumerable<EdgeTo<TEdge, TElement>> NeighborsWithEdges(TElement from)
    {
        return _edges.TryGetValue(from, out var hs) ? hs : Enumerable.Empty<EdgeTo<TEdge, TElement>>();
    }

    public bool AreNeighbors(TElement from, TElement to, TEdge edge)
    {
        if (!_edges.TryGetValue(from, out var collection)) return false;

        return collection.Contains(new EdgeTo<TEdge, TElement>(edge, to));
    }

    public bool AreNeighbors(TElement from, TElement to)
    {
        if (!_edges.TryGetValue(from, out var collection)) return false;

        foreach (var edgeTo in collection)
        {
            if (edgeTo.To.Equals(to)) return true;
        }

        return false;
    }

    public TEdge? LinkBetween(TElement from, TElement to)
    {
        if (!_edges.TryGetValue(from, out var collection)) return default;

        foreach (var edgeTo in collection)
        {
            if (edgeTo.To.Equals(to)) return edgeTo.Edge;
        }

        return default;
    }

    public void Clear()
    {
        _edges.Clear();
    }

    public IEnumerator<TElement> GetEnumerator()
    {
        return _edges.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class HDictionaryGraph<TElement, TEdge> : DictionaryGraph<TElement, TEdge> where TElement : notnull where TEdge : notnull
{
    protected override IContainingCollection<EdgeTo<TEdge, TElement>> CreateCollection() => new ContainingHashSet<EdgeTo<TEdge, TElement>>();
}

public class ULDictionaryGraph<TElement, TEdge> : DictionaryGraph<TElement, TEdge> where TElement : notnull where TEdge : notnull
{
    protected override IContainingCollection<EdgeTo<TEdge, TElement>> CreateCollection() => new UniqueList<EdgeTo<TEdge, TElement>>();
}