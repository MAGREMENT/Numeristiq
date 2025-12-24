using System.Collections.Generic;
using Model.Utility;
using System.Linq;
using Model.Core.Graphs;

namespace Model.Sudokus.Solver.Strategies.BlossomLoops.LoopFinders;

public class BlossomLoopExclusiveDFSLoopFinder : IBlossomLoopLoopFinder
{
    public int MaxLoopLength { get; set; }

    public BlossomLoopExclusiveDFSLoopFinder(int maxLoopSize)
    {
        MaxLoopLength = maxLoopSize;
    }

    public List<Loop<ISudokuElement, LinkStrength>> Find(CellPossibility[] cps, IGraph<ISudokuElement, LinkStrength> graph)
    {
        List<Loop<ISudokuElement, LinkStrength>> result = new();
        Dictionary<ISudokuElement, int> nope = new();

        foreach (var start in cps)
        {
            Search(cps, graph, nope, new ChainBuilder<ISudokuElement, LinkStrength>(start), result);
            nope.Clear();
        }

        return result;
    }

    private bool Search(CellPossibility[] cps, 
        IGraph<ISudokuElement, LinkStrength> graph, 
        Dictionary<ISudokuElement, int> nope,
        ChainBuilder<ISudokuElement, LinkStrength> builder, 
        List<Loop<ISudokuElement, LinkStrength>> result)
    {
        if (builder.Count > MaxLoopLength) return false;
        
        var last = builder.LastElement();
        var first = builder.FirstElement();
        var link = builder.LastLink() == LinkStrength.None ? LinkStrength.Any : builder.LastLink() ==
            LinkStrength.Strong ? LinkStrength.Any : LinkStrength.Strong;

        bool foundSomething = false;
        foreach (var friend in graph.GetNeighbors(last, link))
        {
            if (friend.Equals(first) || (nope.TryGetValue(friend, out var num) && num <= builder.Count)) continue;

            if (friend is CellPossibility cp && cps.Contains(cp))
            {
                if (builder.Count < 3 || builder.Count % 2 != 1) continue;
                
                builder.Add(LinkStrength.Weak, friend);
                result.Add(builder.ToLoop(LinkStrength.Strong));
                foundSomething = true;
                builder.RemoveLast();
            }
            else if (!builder.ContainsElement(friend) && !friend.ContainsAny(cps))
            {
                builder.Add(link == LinkStrength.Any ? LinkStrength.Weak : LinkStrength.Strong, friend);
                if (Search(cps, graph, nope, builder, result)) foundSomething = true;
                builder.RemoveLast();
            }
        }

        if (!foundSomething)
        {
            nope[last] = builder.Count;
        }
        
        return foundSomething;
    }
}