using System;
using System.Collections.Generic;
using System.Linq;

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

    public static List<TLoop> Multiply<TLoop>(IReadOnlyList<TLoop> loops, CombineLoops<TLoop> combiner) where TLoop : class
    {
        List<TLoop> result = new();
        for (int i = 0; i < loops.Count; i++)
        {
            for (int j = i + 1; j < loops.Count; j++)
            {
                var l = combiner(loops[i], loops[j]);
                if (l is not null) result.Add(l);
            }
        }

        return result;
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

        for (int i = 0; i < fullPath.Count; i++)
        {
            elements.Add(fullPath.Elements[i]); 
            links.Add(i == fullPath.Count - 1 ? middleLink : fullPath.Links[i]);
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
        int oneIndex1 = -1;
        int oneIndex2 = -1;
        
        int twoIndex1 = -1;
        int twoIndex2 = -1;

        for (int i = 0; i < one.Count; i++)
        {
            var index = two.IndexOf(one.Elements[i]);
            if (index == -1) continue;

            if (oneIndex1 == -1)
            {
                oneIndex1 = i;
                twoIndex1 = index;
            }
            else if (oneIndex2 == -1)
            {
                oneIndex2 = i;
                twoIndex2 = index;
            }
            else return null;
        }
        
        if (twoIndex1 == -1) return null;

        var elements = new TElement[one.Count + two.Count - 2];
        var links = new LinkStrength[one.Count + two.Count - 2];

        var oneLength = oneIndex2 - oneIndex1 + 1;
        Array.Copy(one.Elements, 0, elements, oneIndex1, oneLength);
        Array.Copy(one.Links, 0, links, oneIndex1, oneLength - 1);

        var diff = twoIndex2 < oneIndex2 ? 1 : -1;
        var cursor1 = oneLength;
        var cursor2 = twoIndex1;

        AdvanceInLoop(ref cursor2, diff, two.Count);
        while (cursor2 != twoIndex2)
        {
            elements[cursor1] = two.Elements[cursor2];
            
            cursor1++;
            AdvanceInLoop(ref cursor2, diff, two.Count);
        }
        
        return new Loop<TElement, LinkStrength>(elements, links);
    }

    private static void AdvanceInLoop(ref int index, int diff, int count)
    {
        index += diff;
        if (index == -1) index = count - 1;
        else if (index == count) index = 0;
    }
}

public delegate TLoop? ConstructLoop<TElement, out TLoop>(Chain<TElement, LinkStrength> fullPath,
    Chain<TElement, LinkStrength> nonFullPath, int index, LinkStrength middleLink, LinkStrength lastLink)
    where TLoop : class where TElement : notnull;
    
public delegate TLoop? CombineLoops<TLoop>(TLoop one, TLoop two);