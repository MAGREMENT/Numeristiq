using System.Collections.Generic;
using System.Linq;
using Global.Enums;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.CellColoring;
using Model.Solver.StrategiesUtility.CellColoring.ColoringAlgorithms;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies.BlossomLoops.LoopFinders;

public class BLLoopFinderV1 : IBlossomLoopLoopFinder
{
    public List<LinkGraphLoop<IChainingElement>> Find(CellPossibility[] cps, ILinkGraph<IChainingElement> graph)
    {
        List<LinkGraphLoop<IChainingElement>> result = new();

        foreach (var start in cps)
        {
            ColoringHistory<IChainingElement> parents = new();
            Queue<ColoredElement<IChainingElement>> queue = new();
            queue.Enqueue(new ColoredElement<IChainingElement>(start, Coloring.On));

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var link = current.Coloring == Coloring.On ? LinkStrength.Any : LinkStrength.Strong;
                var opposite = current.Coloring == Coloring.On ? Coloring.Off : Coloring.On;

                foreach (var friend in graph.Neighbors(current.Element, link))
                {
                    if (friend is CellPossibility cp && cps.Contains(cp))
                    {
                        if (start.Equals(current.Element)) continue;

                        var path = parents.GetPathToRootWithRealLinks(friend, graph);
                        if (path.Count >= 3 && path.Count % 2 == 1)
                        {
                            result.Add(new LinkGraphLoop<IChainingElement>(path.Elements, path.Links,
                                LinkStrength.Strong));
                        }
                    }
                    else
                    {
                        if (parents.ContainsChild(friend)) continue;

                        parents.Add(friend, current.Element);
                        queue.Enqueue(new ColoredElement<IChainingElement>(friend, opposite));
                    }
                }
            }
        }

        return result;
    }

    
}
