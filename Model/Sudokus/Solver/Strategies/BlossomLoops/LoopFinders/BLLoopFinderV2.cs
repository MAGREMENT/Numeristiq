using System.Collections.Generic;
using System.Linq;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies.BlossomLoops.LoopFinders;

public class BLLoopFinderV2 : IBlossomLoopLoopFinder //TODO fix
{
    public List<Loop<ISudokuElement, LinkStrength>> Find(CellPossibility[] cps, ILinkGraph<ISudokuElement> graph)
    {
        List<Loop<ISudokuElement, LinkStrength>> result = new();

        foreach (var cp in cps)
        {
            Search(result, graph, cp, cps);
        }

        return result;
    }

    private void Search(List<Loop<ISudokuElement, LinkStrength>> result, ILinkGraph<ISudokuElement> graph,
        CellPossibility start, CellPossibility[] cps)
    {
        Dictionary<ISudokuElement, ISudokuElement> on = new();
        Dictionary<ISudokuElement, ISudokuElement> off = new();
        Queue<ISudokuElement> queue = new();

        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var neighbor in graph.Neighbors(current))
            {
                if (neighbor.Equals(start)) continue;

                if (neighbor is CellPossibility cp && cps.Contains(cp))
                {
                    var loop = TryMakeLoop(on, off, neighbor, start);
                    if (loop is not null)
                    {
                        result.Add(loop);
                        continue;
                    }
                }

                off.TryAdd(neighbor, current);
                foreach (var secondNeighbor in graph.Neighbors(neighbor, LinkStrength.Strong))
                {
                    if (!ComponentsCheck(cps, secondNeighbor)) continue;

                    if (on.TryAdd(secondNeighbor, neighbor)) queue.Enqueue(secondNeighbor);
                }
            }
        }
    }

    private static Loop<ISudokuElement, LinkStrength>? TryMakeLoop(Dictionary<ISudokuElement, ISudokuElement> on, 
        Dictionary<ISudokuElement, ISudokuElement> off, ISudokuElement end, ISudokuElement start)
    {
        List<ISudokuElement> elements = new();
        List<LinkStrength> links = new();

        elements.Add(end);
        links.Add(LinkStrength.Strong);

        while (true)
        {
            elements.Add(off[elements[^1]]);
            links.Add(LinkStrength.Weak);

            if (!on.TryGetValue(elements[^1], out var value)) break;

            elements.Add(value);
            links.Add(LinkStrength.Strong);
        }

        elements.Add(start);
        links.Add(LinkStrength.Weak);
        if (elements.Count < 4) return null;
        
        elements.Reverse();
        links.Reverse();
        return new Loop<ISudokuElement, LinkStrength>(elements.ToArray(), links.ToArray());
    }
    
    private static bool ComponentsCheck(CellPossibility[] cps, ISudokuElement friend)
    {
        foreach (var cp in friend.EnumerateCellPossibility())
        {
            if (cps.Contains(cp)) return false;
        }

        return true;
    }
}