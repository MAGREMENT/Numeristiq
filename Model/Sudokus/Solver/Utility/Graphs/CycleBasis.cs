using System.Collections.Generic;

namespace Model.Sudokus.Solver.Utility.Graphs;

public static class CycleBasis //TODO test
{
    public static List<TLoop> Find<TElement, TLoop>(ILinkGraph<TElement> graph, ConstructLoop<TElement, TLoop> constructor)
        where TElement : ISudokuElement where TLoop : class
    {
        var result = new List<TLoop>();
        var forest = FindSpanningForest(graph);

        foreach (var start in graph)
        {
            foreach (var friend in graph.Neighbors(start))
            {
                if((forest.TryGetValue(start, out var startSource) && startSource.Equals(friend))
                   || (forest.TryGetValue(friend, out var friendSource) && friendSource.Equals(start))) continue;

                List<TElement> path1 = new();
                List<TElement> path2 = new();

                path1.Add(start);
                path2.Add(friend);

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
                            var index = path2.IndexOf(next1);
                            if (index == -1)
                            {
                                path1.Add(next1);
                                cur1 = next1;
                            }
                            else
                            {
                                var loop = constructor(path1, path2, index);
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
                            var index = path1.IndexOf(next2);
                            if (index == -1)
                            {
                                path2.Add(next2);
                                cur2 = next2;
                            }
                            else
                            {
                                var loop = constructor(path2, path1, index);
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
            }
        }

        return result;
    }

    private static Dictionary<T, T> FindSpanningForest<T>(ILinkGraph<T> graph) where T : notnull
    {
        Dictionary<T, T> result = new();
        Queue<T> queue = new();

        foreach (var start in graph)
        {
            if(result.ContainsKey(start)) continue;

            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var friend in graph.Neighbors(current))
                {
                    if(result.ContainsKey(friend)) continue;

                    result.Add(friend, current);
                    queue.Enqueue(friend);
                }
            }
        }

        return result;
    }
}

public delegate TLoop? ConstructLoop<TElement, out TLoop>(List<TElement> fullPath, List<TElement> nonFullPath,
    int index) where TLoop : class;