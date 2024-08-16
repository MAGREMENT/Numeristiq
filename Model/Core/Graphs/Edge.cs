using System;

namespace Model.Core.Graphs;

public class Edge<T> where T : notnull
{
    public Edge(T from, T to)
    {
        From = from;
        To = to;
    }

    public T From { get; }
    public T To { get; }

    public override int GetHashCode()
    {
        return HashCode.Combine(From.GetHashCode() ^ To.GetHashCode());
    }

    public override bool Equals(object? obj)
    {
        return obj is Edge<T> l &&
               ((l.From.Equals(From) && l.To.Equals(To)) || (l.From.Equals(To) && l.To.Equals(From)));
    }
}

public class EdgeTo<TEdge, TElement>  where TEdge : notnull where TElement : notnull
{
    public EdgeTo(TEdge edge, TElement to)
    {
        Edge = edge;
        To = to;
    }

    public TEdge Edge { get; }
    public TElement To { get; }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Edge.GetHashCode(), To.GetHashCode());
    }

    public override bool Equals(object? obj)
    {
        return obj is EdgeTo<TEdge, TElement> e && e.Edge.Equals(Edge) && e.To.Equals(To);
    }
}