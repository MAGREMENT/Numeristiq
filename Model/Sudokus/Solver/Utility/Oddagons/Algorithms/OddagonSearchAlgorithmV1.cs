using System.Collections.Generic;
using Model.Core.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Oddagons.Algorithms;

public class OddagonSearchAlgorithmV1 : IOddagonSearchAlgorithm
{
    public int MaxLength { get; set; }
    public int MaxGuardians { get; set; }

    public List<AlmostOddagon> Search(ISudokuSolverData solverData, IGraph<CellPossibility, LinkStrength> graph)
    {
        List<AlmostOddagon> result = new();
        HashSet<CellPossibility> done = new();
        foreach (var start in graph)
        {
            Search(solverData, new ChainBuilder<CellPossibility, LinkStrength>(start),
                new List<CellPossibility>(), done, graph, result);

            done.Add(start);
        }

        return result;
    }

    private void Search(ISudokuSolverData solverData, ChainBuilder<CellPossibility, LinkStrength> builder,
        List<CellPossibility> currentGuardians, HashSet<CellPossibility> done,
        IGraph<CellPossibility, LinkStrength> graph, List<AlmostOddagon> result)
    {
        if (builder.Count > MaxLength) return;
        
        var last = builder.LastElement();
        var first = builder.FirstElement();

        foreach (var friend in graph.Neighbors(last, LinkStrength.Strong))
        {
            if (done.Contains(friend) || currentGuardians.Contains(friend)) continue;
            
            if (friend == first)
            {
                if (builder.Count < 3 || builder.Count % 2 != 1) continue;

                result.Add(new AlmostOddagon(builder.ToLoop(LinkStrength.Strong), currentGuardians.ToArray()));
            }
            else if (!builder.ContainsElement(friend))
            {
                builder.Add(LinkStrength.Strong, friend);
                Search(solverData, builder, currentGuardians, done, graph, result);
                builder.RemoveLast();
            }
        }
        
        foreach (var friend in graph.Neighbors(last, LinkStrength.Weak))
        {
            if (done.Contains(friend) || currentGuardians.Contains(friend)) continue;
            
            bool ok = true;
            var count = 0;
            foreach (var guardian in OddagonSearcher.FindGuardians(solverData, last, friend))
            {
                if (currentGuardians.Contains(guardian)) continue;
                if (builder.ContainsElement(guardian))
                {
                    ok = false;
                    break;
                }
                
                currentGuardians.Add(guardian);
                count++;
            }

            if (ok && currentGuardians.Count <= MaxGuardians)
            {
                if (friend == first)
                {
                    if (builder.Count < 5 || builder.Count % 2 != 1) continue;

                    result.Add(new AlmostOddagon(builder.ToLoop(LinkStrength.Weak), currentGuardians.ToArray()));
                }
                else if (!builder.ContainsElement(friend))
                {
                    builder.Add(LinkStrength.Weak, friend);
                    Search(solverData, builder, currentGuardians, done, graph, result);
                    builder.RemoveLast();
                }
            }

            currentGuardians.RemoveRange(currentGuardians.Count - count, count);
        }
    }
}