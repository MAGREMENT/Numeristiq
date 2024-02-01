using System.Collections.Generic;
using System.Linq;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.CellColoring;
using Model.Sudoku.Solver.StrategiesUtility.CellColoring.ColoringAlgorithms;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;

namespace Model.Sudoku.Solver.Strategies.BlossomLoops.LoopFinders;

public class BLLoopFinderV1 : IBlossomLoopLoopFinder
{
    public List<LinkGraphLoop<ISudokuElement>> Find(CellPossibility[] cps, ILinkGraph<ISudokuElement> graph)
    {
        List<LinkGraphLoop<ISudokuElement>> result = new();

        foreach (var start in cps)
        {
            ColoringHistory<ISudokuElement> parents = new();
            Queue<ColoredElement<ISudokuElement>> queue = new();
            queue.Enqueue(new ColoredElement<ISudokuElement>(start, Coloring.On));

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
                            result.Add(new LinkGraphLoop<ISudokuElement>(path.Elements, path.Links,
                                LinkStrength.Strong));
                        }
                    }
                    else
                    {
                        if (parents.ContainsChild(friend)) continue;

                        parents.Add(friend, current.Element);
                        queue.Enqueue(new ColoredElement<ISudokuElement>(friend, opposite));
                    }
                }
            }
        }

        return result;
    }

    
}
