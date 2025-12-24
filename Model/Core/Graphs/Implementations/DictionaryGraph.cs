using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.Utility.Collections;

namespace Model.Core.Graphs.Implementations;

public class DictionaryGraph<TElement, TEdge, TCollection> : IGraph<TElement, TEdge> 
    where TElement : notnull where TEdge : notnull where TCollection : IContainingCollection<EdgeTo<TEdge, TElement>>, new()
{
    private readonly Dictionary<TElement, TCollection> _edges = new();
    
    public void Add(TElement from, TElement to, TEdge edge, LinkType type = LinkType.BiDirectional)
    {
        if (!_edges.TryGetValue(from, out var hs))
        {
            hs = new TCollection();
            _edges[from] = hs;
        }

        hs.Add(new EdgeTo<TEdge, TElement>(edge, to));
        
        if (type != LinkType.BiDirectional) return;
        
        if (!_edges.TryGetValue(to, out hs))
        {
            hs = new TCollection();
            _edges[to] = hs;
        }
        hs.Add(new EdgeTo<TEdge, TElement>(edge, from));
    }

    public IEnumerable<TElement> GetNeighbors(TElement from, TEdge edge)
    {
        foreach (var edgeTo in GetNeighborsWithEdges(from))
        {
            if (edgeTo.Edge.Equals(edge)) yield return edgeTo.To;
        }
    }

    public IEnumerable<TElement> GetNeighbors(TElement from)
    {
        foreach (var edgeTo in GetNeighborsWithEdges(from))
        {
            yield return edgeTo.To;
        }
    }

    public IEnumerable<EdgeTo<TEdge, TElement>> GetNeighborsWithEdges(TElement from)
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

    public TEdge? GetEdge(TElement from, TElement to)
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