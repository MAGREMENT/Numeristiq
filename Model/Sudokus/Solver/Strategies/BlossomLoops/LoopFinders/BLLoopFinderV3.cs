﻿using System.Collections.Generic;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using System.Linq;
using Model.Core.Graphs;

namespace Model.Sudokus.Solver.Strategies.BlossomLoops.LoopFinders;

public class BLLoopFinderV3 : IBlossomLoopLoopFinder
{
    private readonly int _maxLoopSize;

    public BLLoopFinderV3(int maxLoopSize)
    {
        _maxLoopSize = maxLoopSize;
    }

    public List<Loop<ISudokuElement, LinkStrength>> Find(CellPossibility[] cps, IGraph<ISudokuElement, LinkStrength> graph)
    {
        List<Loop<ISudokuElement, LinkStrength>> result = new();
        HashSet<ISudokuElement> nope = new();

        foreach (var start in cps)
        {
            Search(cps, graph, nope, new ChainBuilder<ISudokuElement, LinkStrength>(start), result);
            nope.Clear();
        }

        return result;
    }

    private bool Search(CellPossibility[] cps, IGraph<ISudokuElement, LinkStrength> graph, HashSet<ISudokuElement> nope,
        ChainBuilder<ISudokuElement, LinkStrength> builder, List<Loop<ISudokuElement, LinkStrength>> result)
    {
        if (builder.Count > _maxLoopSize) return false;
        
        var last = builder.LastElement();
        var first = builder.FirstElement();
        var link = builder.LastLink() == LinkStrength.None ? LinkStrength.Any : builder.LastLink() ==
            LinkStrength.Strong ? LinkStrength.Any : LinkStrength.Strong;

        bool foundSomething = false;
        foreach (var friend in graph.Neighbors(last, link))
        {
            if (friend.Equals(first) || nope.Contains(friend)) continue;

            if (friend is CellPossibility cp && cps.Contains(cp))
            {
                if (builder.Count < 3 || builder.Count % 2 != 1) continue;
                
                builder.Add(LinkStrength.Weak, friend);
                result.Add(builder.ToLoop(LinkStrength.Strong));
                foundSomething = true;
                builder.RemoveLast();
            }
            else if (!builder.ContainsElement(friend) && ComponentsCheck(cps, friend))
            {
                builder.Add(link == LinkStrength.Any ? LinkStrength.Weak : LinkStrength.Strong, friend);
                if (Search(cps, graph, nope, builder, result)) foundSomething = true;
                builder.RemoveLast();
            }
        }

        if (!foundSomething) nope.Add(last);
        return foundSomething;
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