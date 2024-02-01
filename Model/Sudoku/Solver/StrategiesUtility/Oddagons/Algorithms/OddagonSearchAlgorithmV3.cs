using System.Collections.Generic;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;

namespace Model.Sudoku.Solver.StrategiesUtility.Oddagons.Algorithms;

public class OddagonSearchAlgorithmV3 : IOddagonSearchAlgorithm
{
    private readonly int _maxLength;
    private readonly int _maxGuardians;

    public OddagonSearchAlgorithmV3(int maxLength, int maxGuardians)
    {
        _maxLength = maxLength;
        _maxGuardians = maxGuardians;
    }


    public List<AlmostOddagon> Search(IStrategyManager strategyManager, ILinkGraph<CellPossibility> graph)
    {
        List<AlmostOddagon> result = new();
        foreach (var start in graph)
        {
            Search(strategyManager, new LinkGraphChainBuilder<CellPossibility>(start),
                new List<CellPossibility>(), graph, result);
        }

        return result;
    }

    private void Search(IStrategyManager strategyManager, LinkGraphChainBuilder<CellPossibility> builder,
        List<CellPossibility> currentGuardians, ILinkGraph<CellPossibility> graph, List<AlmostOddagon> result)
    {
        if (builder.Count > _maxLength) return;
        
        var last = builder.LastElement();
        var first = builder.FirstElement();

        foreach (var friend in graph.Neighbors(last, LinkStrength.Strong))
        {
            if (currentGuardians.Contains(friend)) continue;
            
            if (friend == first)
            {
                if (builder.Count < 5 || builder.Count % 2 != 1) continue;

                result.Add(new AlmostOddagon(builder.ToLoop(LinkStrength.Strong), currentGuardians.ToArray()));
            }
            else if (!builder.ContainsElement(friend))
            {
                builder.Add(LinkStrength.Strong, friend);
                Search(strategyManager, builder, currentGuardians, graph, result);
                builder.RemoveLast();
            }
        }
        
        foreach (var friend in graph.Neighbors(last, LinkStrength.Weak))
        {
            if (currentGuardians.Contains(friend)) continue;

            bool ok = true;
            var count = 0;
            foreach (var guardian in OddagonSearcher.FindGuardians(strategyManager, last, friend))
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

            if (ok && currentGuardians.Count <= _maxGuardians)
            {
                if (friend == first)
                {
                    if (builder.Count < 5 || builder.Count % 2 != 1) continue;

                    result.Add(new AlmostOddagon(builder.ToLoop(LinkStrength.Weak), currentGuardians.ToArray()));
                }
                else if (!builder.ContainsElement(friend))
                {
                    builder.Add(LinkStrength.Weak, friend);
                    Search(strategyManager, builder, currentGuardians, graph, result);
                    builder.RemoveLast();
                }  
            }
            
            currentGuardians.RemoveRange(currentGuardians.Count - count, count);
        }
    }
}