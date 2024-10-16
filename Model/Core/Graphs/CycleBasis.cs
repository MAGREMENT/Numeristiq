using System.Collections.Generic;
using System.Linq;
using Model.Utility.BitSets;

namespace Model.Core.Graphs;

public static class CycleBasis //TODO test
{
    public static List<TLoop> Find<TElement, TLoop>(IGraph<TElement, LinkStrength> graph, ConstructLoop<TElement, TLoop> constructor)
        where TLoop : class where TElement : notnull
    {
        var result = new List<TLoop>();
        var forest = FindSpanningForest(graph);
        HashSet<Edge<TElement>> done = new();

        foreach (var start in graph)
        {
            foreach (var friend in graph.Neighbors(start))
            {
                if((forest.TryGetValue(start, out var startSource) && startSource.To.Equals(friend))
                   || (forest.TryGetValue(friend, out var friendSource) && friendSource.To.Equals(start))) continue;

                var link = new Edge<TElement>(start, friend);
                if(done.Contains(link)) continue;
                
                ChainBuilder<TElement, LinkStrength> path1 = new(start);
                ChainBuilder<TElement, LinkStrength> path2 = new(friend);

                var cur1 = start;
                var continue1 = true;
                var cur2 = friend;
                var continue2 = true;

                do
                {
                    if (continue1)
                    {
                        if (forest.TryGetValue(cur1, out var next1))
                        {
                            var index = path2.IndexOf(next1.To);
                            if (index == -1)
                            {
                                path1.Add(next1.Edge, next1.To);
                                cur1 = next1.To;
                            }
                            else
                            {
                                var loop = constructor(path1.ToChain(), path2.ToChain(), index,
                                    graph.LinkBetween(cur1, next1.To), 
                                    graph.LinkBetween(start, friend));
                                if (loop is not null)
                                {
                                    result.Add(loop);
                                    break;
                                }
                            }
                        }
                        else continue1 = false;
                    }

                    if (continue2)
                    {
                        if (forest.TryGetValue(cur2, out var next2))
                        {
                            var index = path1.IndexOf(next2.To);
                            if (index == -1)
                            {
                                path2.Add(next2.Edge, next2.To);
                                cur2 = next2.To;
                            }
                            else
                            {
                                var loop = constructor(path2.ToChain(), path1.ToChain(), index,
                                    graph.LinkBetween(cur2, next2.To),
                                    graph.LinkBetween(start, friend));
                                if (loop is not null)
                                {
                                    result.Add(loop);
                                    break;
                                }
                            }
                        }
                        else continue2 = false;
                    }
                } while (continue1 && continue2);

                done.Add(link);
            }
        }

        return result;
    }

    public static IEnumerable<TLoop> Multiply<TLoop>(IReadOnlyList<TLoop> basis, CombineLoops<TLoop> combiner, int iterations)
    {
        if (iterations <= 0) return basis;

        HashSet<TLoop> result = new(basis);
        if (iterations == 1)
        {
            result.UnionWith(CombineAll(basis, combiner));
            return result;
        }

        HashSet<TLoop> added = new();
        foreach (var loop in CombineAll(basis, combiner))
        {
            if (!result.Contains(loop)) added.Add(loop);
        }

        if (added.Count == 0) return result;
        
        for (int i = 1; i < iterations; i++)
        {
            HashSet<TLoop> next = new();

            foreach (var l1 in result)
            {
                foreach (var l2 in added)
                {
                    var l = combiner(l1, l2);
                    if (l is not null && !added.Contains(l) && !result.Contains(l)) next.Add(l);
                }
            }

            result.UnionWith(added);
            if (next.Count == 0) return result;

            added = next;
        }

        result.UnionWith(added);
        return result;
    }

    private static IEnumerable<TLoop> CombineAll<TLoop>(IReadOnlyList<TLoop> basis, CombineLoops<TLoop> combiner)
    {
        for (int i = 0; i < basis.Count; i++)
        {
            for (int j = i + 1; j < basis.Count; j++)
            {
                var l = combiner(basis[i], basis[j]);
                if (l is not null) yield return l;
            }
        }
    }

    private static Dictionary<TElement, EdgeTo<LinkStrength, TElement>> FindSpanningForest<TElement>(IGraph<TElement, LinkStrength> graph)
        where TElement : notnull
    {
        Dictionary<TElement, EdgeTo<LinkStrength, TElement>> result = new();
        HashSet<TElement> starts = new();
        Queue<TElement> queue = new();

        foreach (var start in graph)
        {
            if(result.ContainsKey(start) || starts.Contains(start)) continue;

            starts.Add(start);
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var friend in graph.Neighbors(current))
                {
                    if(result.ContainsKey(friend) || starts.Contains(friend)) continue;

                    var link = graph.LinkBetween(friend, current);
                    if(link is LinkStrength.None) continue;
                    
                    result.Add(friend, new EdgeTo<LinkStrength, TElement>(link, current));
                    queue.Enqueue(friend);
                }
            }
        }

        return result;
    }
    
    public static Loop<T, LinkStrength> DefaultConstructLoop<T>(Chain<T, LinkStrength> fullPath, Chain<T, LinkStrength> nonFullPath,
        int index, LinkStrength middleLink, LinkStrength lastLink) where T : notnull
    {
        List<T> elements = new();
        List<LinkStrength> links = new();

        for (int i = 0; i < fullPath.Elements.Count; i++)
        {
            elements.Add(fullPath.Elements[i]); 
            links.Add(i == fullPath.Elements.Count - 1 ? middleLink : fullPath.Links[i]);
        }

        for (int i = index; i >= 0; i--)
        {
            elements.Add(nonFullPath.Elements[i]);
            links.Add(i == 0 ? lastLink : nonFullPath.Links[i - 1]);
        }

        return new Loop<T, LinkStrength>(elements.ToArray(), links.ToArray());
    }

    public static Loop<TElement, LinkStrength>? DefaultCombineLoops<TElement>(Loop<TElement, LinkStrength> one,
        Loop<TElement, LinkStrength> two) where TElement : notnull
    {
        Dictionary<TElement, EdgeTo<LinkStrength, TElement>> edges = new();
        for (int i = 0; i < one.Elements.Count; i++)
        {
            edges.Add(one.Elements[i], new EdgeTo<LinkStrength, TElement>(
                one.Links[i],
                one.Elements[i < one.Elements.Count - 1 ? i + 1 : 0]));
        }

        TElement curr;
        TElement next;
        //-1 = undetermined, 0 = same, 1 = opposite
        int cycle = -1;
        InfiniteBitSet ignored = new();

        for (int i = 0; i < two.Elements.Count; i++)
        {
            curr = two.Elements[i];
            var ind = i < two.Elements.Count - 1 ? i + 1 : 0;
            next = two.Elements[ind];

            if (edges.TryGetValue(curr, out var v) && v.To.Equals(next))
            {
                if (!v.Edge.Equals(two.Links[i])) return null;
                
                edges.Remove(curr);
                ignored.Add(i);
                
                switch (cycle)
                {
                    case -1:
                        cycle = 1;
                        break;
                    case 0:
                        return null;
                }

                continue;
            }

            if (edges.TryGetValue(next, out v) && v.To.Equals(curr))
            {
                if (!v.Edge.Equals(two.Links[i])) return null;
                
                edges.Remove(next);
                ignored.Add(ind);
                
                switch (cycle)
                {
                    case -1:
                        cycle = 0;
                        break;
                    case 1:
                        return null;
                }
            }
        }

        switch (cycle)
        {
            case -1:
                return null;
            case 0:
            {
                for (int i = 0; i < two.Elements.Count; i++)
                {
                    curr = two.Elements[i];
                    var ind = i < two.Elements.Count - 1 ? i + 1 : 0;
                    next = two.Elements[ind];

                    if (!ignored.Contains(ind) &&
                        !edges.TryAdd(curr, new EdgeTo<LinkStrength, TElement>(two.Links[i], next))) return null;
                }

                break;
            }
            default:
            {
                for (int i = two.Elements.Count - 1; i >= 0; i--)
                {
                    curr = two.Elements[i];
                    var ind = i == 0 ? two.Elements.Count - 1 : i - 1;
                    next = two.Elements[ind];

                    if (!ignored.Contains(ind) &&
                        !edges.TryAdd(curr, new EdgeTo<LinkStrength, TElement>(two.Links[i], next))) return null;
                }

                break;
            }
        }

        if (edges.Count <= 2) return null;

        var elements = new TElement[edges.Count];
        var links = new LinkStrength[edges.Count];
        curr = edges.Keys.First();
        elements[0] = curr;

        EdgeTo<LinkStrength, TElement>? buffer;
        for (int i = 1; i < elements.Length; i++)
        {
            if (!edges.TryGetValue(curr, out buffer)) return null;

            elements[i] = buffer.To;
            links[i - 1] = buffer.Edge;
            curr = buffer.To;
        }

        buffer = edges[elements[^1]];
        if (!buffer.To.Equals(elements[0])) return null;

        links[^1] = buffer.Edge;
        return new Loop<TElement, LinkStrength>(elements, links);
    }
}

public delegate TLoop? ConstructLoop<TElement, out TLoop>(Chain<TElement, LinkStrength> fullPath,
    Chain<TElement, LinkStrength> nonFullPath, int index, LinkStrength middleLink, LinkStrength lastLink)
    where TLoop : class where TElement : notnull;
    
public delegate TLoop? CombineLoops<TLoop>(TLoop one, TLoop two);