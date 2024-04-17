using System.Collections.Generic;
using System.Linq;
using Model.Sudokus.Solver.StrategiesUtility;
using Model.Sudokus.Solver.StrategiesUtility.CellColoring;
using Model.Sudokus.Solver.StrategiesUtility.CellColoring.ColoringAlgorithms;
using Model.Sudokus.Solver.StrategiesUtility.Graphs;

namespace Model.Sudokus.Solver.Strategies.BlossomLoops.BranchFinder;

public class BLBranchFinderV1 : IBlossomLoopBranchFinder
{
    public BlossomLoopBranch[]? FindShortestBranches(ILinkGraph<ISudokuElement> graph,
        CellPossibility[] cps, LinkGraphLoop<ISudokuElement> loop)
    {
        HashSet<ISudokuElement> nope = new(loop.Elements);

        var result = new BlossomLoopBranch[cps.Length - 2];
        var cursor = 0;
        foreach (var cp in cps)
        {
            if (loop.Contains(cp)) continue;
            
            ColoringHistory<ISudokuElement> parents = new();
            Queue<ColoredElement<ISudokuElement>> queue = new();
            queue.Enqueue(new ColoredElement<ISudokuElement>(cp, Coloring.On));
            bool ok = true;

            while (queue.Count > 0 && ok)
            {
                var current = queue.Dequeue();
                var link = current.Coloring == Coloring.On ? LinkStrength.Any : LinkStrength.Strong;
                var opposite = current.Coloring == Coloring.On ? Coloring.Off : Coloring.On;

                foreach (var friend in graph.Neighbors(current.Element, link))
                {
                    if (ContainsAnyCellPossibility(cps, friend) || nope.Contains(friend) || parents.ContainsChild(friend)) continue;

                    parents.Add(friend, current.Element);
                    queue.Enqueue(new ColoredElement<ISudokuElement>(friend, opposite));

                    bool isBranch = false;
                    int i = 0;
                    for (; i < loop.Elements.Length; i += 2)
                    {
                        if (graph.AreNeighbors(loop.Elements[i], friend) &&
                            graph.AreNeighbors(loop.Elements[i + 1], friend))
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

    private bool ContainsAnyCellPossibility(CellPossibility[] cps, ISudokuElement element)
    {
        if (element is CellPossibility a) return cps.Contains(a);
        
        foreach (var cp in element.EveryCellPossibility())
        {
            if (cps.Contains(cp)) return true;
        }

        return false;
    }

    private bool CheckTargetOverlap(BlossomLoopBranch[] branches, int cursor, BlossomLoopBranch current, 
        ILinkGraph<ISudokuElement> graph)
    {
        for (int i = 0; i < cursor; i++)
        {
            var b = branches[i];
            if (b.Targets[0].Equals(current.Targets[0]) && b.Targets[1].Equals(current.Targets[1]) &&
                !graph.AreNeighbors(b.Branch.Elements[^1], current.Branch.Elements[^1])) return false;
        }

        return true;
    }
}