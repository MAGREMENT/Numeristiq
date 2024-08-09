using System.Collections.Generic;
using System.Linq;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies.BlossomLoops.LoopFinders;

public class BLLoopFinderV1 : IBlossomLoopLoopFinder
{
    private readonly int _maxLoopSize;

    public BLLoopFinderV1(int maxLoopSize)
    {
        _maxLoopSize = maxLoopSize;
    }

    public List<Loop<ISudokuElement, LinkStrength>> Find(CellPossibility[] cps, ILinkGraph<ISudokuElement> graph)
    {
        List<Loop<ISudokuElement, LinkStrength>> result = new();

        foreach (var start in cps)
        {
            Search(cps, graph, new ChainBuilder<ISudokuElement, LinkStrength>(start), result);
        }

        return result;
    }

    private void Search(CellPossibility[] cps, ILinkGraph<ISudokuElement> graph,
        ChainBuilder<ISudokuElement, LinkStrength> builder, List<Loop<ISudokuElement, LinkStrength>> result)
    {
        if (builder.Count > _maxLoopSize) return;
        
        var last = builder.LastElement();
        var first = builder.FirstElement();
        var link = builder.LastLink() == LinkStrength.None ? LinkStrength.Any : builder.LastLink() ==
            LinkStrength.Strong ? LinkStrength.Any : LinkStrength.Strong;

        foreach (var friend in graph.Neighbors(last, link))
        {
            if (friend.Equals(first)) continue;

            if (friend is CellPossibility cp && cps.Contains(cp))
            {
                if (builder.Count < 3 || builder.Count % 2 != 1) continue;
                
                builder.Add(LinkStrength.Weak, friend);
                result.Add(builder.ToLoop(LinkStrength.Strong));
                builder.RemoveLast();
            }
            else if (!builder.ContainsElement(friend) && ComponentsCheck(cps, friend))
            {
                builder.Add(link == LinkStrength.Any ? LinkStrength.Weak : LinkStrength.Strong, friend);
                Search(cps, graph, builder, result);
                builder.RemoveLast();
            }
        }
    }
    
    private bool ComponentsCheck(CellPossibility[] cps, ISudokuElement friend)
    {
        if (friend is CellPossibility) return true;

        foreach (var cp in friend.EnumerateCellPossibility())
        {
            if (cps.Contains(cp)) return false;
        }

        return true;
    }
}