using System;
using System.Collections.Generic;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;

namespace Model.Sudoku.Solver.StrategiesUtility.Oddagons.Algorithms;

public class OddagonSearchAlgorithmV1 : IOddagonSearchAlgorithm
{
    public List<AlmostOddagon> Search(IStrategyManager strategyManager, ILinkGraph<CellPossibility> graph)
    {
        
        var result = new List<AlmostOddagon>();

        Queue<CellPossibility> queue = new();
        Dictionary<CellPossibility, CellPossibility> parents = new();
        foreach (var start in graph)
        {
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                bool hasParent = parents.TryGetValue(current, out var currentParent);
                
                foreach (var friend in graph.Neighbors(current))
                {
                    if (friend == start ||(hasParent && friend == currentParent)) continue;
                    
                    if (parents.ContainsKey(friend))
                    {
                        var path1 = PathToRoot(parents, current);
                        var path2 = PathToRoot(parents, friend);

                        var supposedTotal = path1.Length + path2.Length - 1;
                        if (supposedTotal < 5 || supposedTotal % 2 != 1) continue;

                        var set = new HashSet<CellPossibility>(path1);
                        set.UnionWith(path2);
                        if (set.Count != supposedTotal) continue;

                        var elements = new CellPossibility[supposedTotal];
                        Array.Reverse(path2);
                        Array.Copy(path1, 0, elements, 0, path1.Length);
                        Array.Copy(path2, 1, elements, path1.Length, path2.Length - 1);
                        
                        result.Add(AlmostOddagon.FromBoard(strategyManager, new LinkGraphLoop<CellPossibility>(elements,
                            SearchLinks(elements, graph))));
                    }
                    else
                    {
                        parents.Add(friend, current);
                        queue.Enqueue(friend);
                    }
                }
            }
            
            queue.Clear();
            parents.Clear();
        }

        return result;
    }

    private CellPossibility[] PathToRoot(Dictionary<CellPossibility, CellPossibility> parents, CellPossibility from)
    {
        List<CellPossibility> result = new();
        result.Add(from);

        var current = from;
        while (parents.TryGetValue(current, out var parent))
        {
            result.Add(parent);
            current = parent;
        }

        return result.ToArray();
    }

    private LinkStrength[] SearchLinks(CellPossibility[] elements, ILinkGraph<CellPossibility> graph)
    {
        var result = new LinkStrength[elements.Length];

        for (int i = 0; i < elements.Length - 1; i++)
        {
            result[i] = graph.AreNeighbors(elements[i], elements[i + 1], LinkStrength.Strong)
                ? LinkStrength.Strong
                : LinkStrength.Weak;
        }
        
        result[^1] = graph.AreNeighbors(elements[^1], elements[0], LinkStrength.Strong)
            ? LinkStrength.Strong
            : LinkStrength.Weak;
        
        return result;
    }
}