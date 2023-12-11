using System.Collections.Generic;
using System.Linq;
using Global.Enums;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.CellColoring;
using Model.Solver.StrategiesUtility.CellColoring.ColoringAlgorithms;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies.BlossomLoops.BranchFinder;

public class BLBranchFinderV1 : IBlossomLoopBranchFinder
{
    public BlossomLoopBranch[]? FindShortestBranches(LinkGraph<ILinkGraphElement> graph,
        CellPossibility[] cps, LinkGraphLoop<ILinkGraphElement> loop)
    {
        HashSet<ILinkGraphElement> nope = new(loop.Elements);

        var result = new BlossomLoopBranch[cps.Length - 2];
        var cursor = 0;
        foreach (var cp in cps)
        {
            if (loop.Contains(cp)) continue;
            
            ColoringHistory<ILinkGraphElement> parents = new();
            Queue<ColoredElement<ILinkGraphElement>> queue = new();
            queue.Enqueue(new ColoredElement<ILinkGraphElement>(cp, Coloring.On));
            bool ok = true;

            while (queue.Count > 0 && ok)
            {
                var current = queue.Dequeue();
                var link = current.Coloring == Coloring.On ? LinkStrength.Any : LinkStrength.Strong;
                var opposite = current.Coloring == Coloring.On ? Coloring.Off : Coloring.On;

                foreach (var friend in graph.GetLinks(current.Element, link))
                {
                    if (ContainsAnyCellPossibility(cps, friend) || nope.Contains(friend) || parents.ContainsChild(friend)) continue;

                    parents.Add(friend, current.Element);
                    queue.Enqueue(new ColoredElement<ILinkGraphElement>(friend, opposite));

                    bool isBranch = false;
                    int i = 0;
                    for (; i < loop.Elements.Length; i += 2)
                    {
                        if (graph.HasLinkTo(loop.Elements[i], friend) &&
                            graph.HasLinkTo(loop.Elements[i + 1], friend))
                        {
                            isBranch = true;
                            break;
                        }
                    }

                    if (!isBranch) continue;
                    
                    var path = parents.GetPathToRootWithGuessedLinks(friend, opposite);
                    if (path.Count < 3 || path.Count % 2 != 1) continue;

                    var branch = new BlossomLoopBranch(path, loop.Elements[i], loop.Elements[i + 1]);
                    if (!CheckTargetOverlap(result, cursor, branch, graph)) continue;
                    
                    result[cursor++] = branch;
                    ok = false;
                    nope.UnionWith(path.Elements);
                    break;
                }
            }

            if (ok) return null;
        }

        return result;
    }

    private bool ContainsAnyCellPossibility(CellPossibility[] cps, ILinkGraphElement element)
    {
        if (element is CellPossibility a) return cps.Contains(a);
        
        foreach (var cp in element.EveryCellPossibility())
        {
            if (cps.Contains(cp)) return true;
        }

        return false;
    }

    private bool CheckTargetOverlap(BlossomLoopBranch[] branches, int cursor, BlossomLoopBranch current, 
        LinkGraph<ILinkGraphElement> graph)
    {
        for (int i = 0; i < cursor; i++)
        {
            var b = branches[i];
            if (b.Targets[0].Equals(current.Targets[0]) && b.Targets[1].Equals(current.Targets[1]) &&
                !graph.HasLinkTo(b.Branch.Elements[^1], current.Branch.Elements[^1])) return false;
        }

        return true;
    }
}