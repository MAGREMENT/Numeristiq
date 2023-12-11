using System.Collections.Generic;
using System.Linq;
using Global.Enums;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies.BlossomLoops.LoopFinders;

public class BLLoopFinderV2 : IBlossomLoopLoopFinder
{
    private readonly int _maxLoopSize;

    public BLLoopFinderV2(int maxLoopSize)
    {
        _maxLoopSize = maxLoopSize;
    }

    public List<LinkGraphLoop<ILinkGraphElement>> Find(CellPossibility[] cps, LinkGraph<ILinkGraphElement> graph)
    {
        List<LinkGraphLoop<ILinkGraphElement>> result = new();

        foreach (var start in cps)
        {
            Search(cps, graph, new LinkGraphChainBuilder<ILinkGraphElement>(start), result);
        }

        return result;
    }

    public void Search(CellPossibility[] cps, LinkGraph<ILinkGraphElement> graph,
        LinkGraphChainBuilder<ILinkGraphElement> builder, List<LinkGraphLoop<ILinkGraphElement>> result)
    {
        if (builder.Count > _maxLoopSize) return;
        
        var last = builder.LastElement();
        var first = builder.FirstElement();
        var link = builder.LastLink() == LinkStrength.None ? LinkStrength.Any : (builder.LastLink() ==
                LinkStrength.Strong ? LinkStrength.Any : LinkStrength.Strong);

        foreach (var friend in graph.GetLinks(last, link))
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
    
    private bool ComponentsCheck(CellPossibility[] cps, ILinkGraphElement friend)
    {
        if (friend is CellPossibility) return true;

        foreach (var cp in friend.EveryCellPossibility())
        {
            if (cps.Contains(cp)) return false;
        }

        return true;
    }
}