using System.Collections.Generic;
using Model.Core.Graphs;
using Model.Core.Graphs.Coloring;
using Model.Core.Graphs.Coloring.ColoringAlgorithms;
using Model.Sudokus.Solver.Utility;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies.BlossomLoops.BranchFinder;

public class BlossomLoopBFSBranchFinder : IBlossomLoopBranchFinder
{
    public BlossomLoopBranch[]? FindShortestBranches(IGraph<ISudokuElement, LinkStrength> graph,
        CellPossibility[] cps, Loop<ISudokuElement, LinkStrength> loop)
    {
        HashSet<ISudokuElement> nope = new(loop.Elements);

        var result = new BlossomLoopBranch[cps.Length - 2];
        var cursor = 0;
        foreach (var cp in cps)
        {
            if (loop.Contains(cp)) continue;
            
            ColoringHistory<ISudokuElement> parents = new();
            Queue<ColoredElement<ISudokuElement>> queue = new();
            queue.Enqueue(new ColoredElement<ISudokuElement>(cp, ElementColor.On));
            
            var step = 1;
            while (queue.Count > 0)
            {
                step++;
                var count = queue.Count;

                for (int n = 0; n < count; n++)
                {
                    var current = queue.Dequeue();
                    var link = current.Coloring == ElementColor.On ? LinkStrength.Any : LinkStrength.Strong;
                    var opposite = current.Coloring == ElementColor.On ? ElementColor.Off : ElementColor.On;

                    foreach (var friend in graph.Neighbors(current.Element, link))
                    {
                        if (nope.Contains(friend) || parents.ContainsChild(friend)) continue;
                        if (friend.ContainsAny(cps))
                        {
                            nope.Add(friend);
                            continue;
                        }

                        parents.Add(friend, current.Element);
                        queue.Enqueue(new ColoredElement<ISudokuElement>(friend, opposite));
                        
                        if (step < 3 || step % 2 != 1) continue;

                        bool isBranch = false;
                        int i = 0;
                        for (; i < loop.Elements.Count; i += 2)
                        {
                            if (graph.AreNeighbors(loop.Elements[i], friend) &&
                                graph.AreNeighbors(loop.Elements[i + 1], friend))
                            {
                                isBranch = true;
                                break;
                            }
                        }

                        if (!isBranch) continue;
                        
                        var branch = new BlossomLoopBranch(parents.GetPathToRootWithGuessedLinks(friend, opposite), 
                            loop.Elements[i], loop.Elements[i + 1]);
                        if (!CheckTargetOverlap(result, cursor, branch, graph)) continue;
                    
                        result[cursor++] = branch;
                        nope.UnionWith(branch.Branch.Elements);

                        goto found;
                    }
                }
            }

            return null;
            found : ;
        }

        return result;
    }

    private bool CheckTargetOverlap(BlossomLoopBranch[] branches, int cursor, BlossomLoopBranch current, 
        IGraph<ISudokuElement, LinkStrength> graph)
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