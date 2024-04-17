using System.Collections.Generic;
using Model.Sudokus.Solver.StrategiesUtility.Graphs;

namespace Model.Sudokus.Solver.StrategiesUtility.Oddagons.Algorithms;

public class OddagonSearchAlgorithmV2 : IOddagonSearchAlgorithm
{
    private readonly int _maxLength;

    public OddagonSearchAlgorithmV2(int maxLength)
    {
        _maxLength = maxLength;
    }


    public List<AlmostOddagon> Search(IStrategyUser strategyUser, ILinkGraph<CellPossibility> graph)
    {
        List<AlmostOddagon> result = new();
        foreach (var start in graph)
        {
            Search(strategyUser, new LinkGraphChainBuilder<CellPossibility>(start), graph, result);
        }

        return result;
    }

    private void Search(IStrategyUser strategyUser, LinkGraphChainBuilder<CellPossibility> builder,
        ILinkGraph<CellPossibility> graph, List<AlmostOddagon> result)
    {
        if (builder.Count > _maxLength) return;
        
        var last = builder.LastElement();
        var first = builder.FirstElement();

        foreach (var friend in graph.Neighbors(last))
        {
            var link = graph.AreNeighbors(last, friend, LinkStrength.Strong) ? LinkStrength.Strong : LinkStrength.Weak;

            if (friend == first)
            {
                if (builder.Count < 5 || builder.Count % 2 != 1) continue;

                result.Add(AlmostOddagon.FromBoard(strategyUser, builder.ToLoop(link)));
            }
            else if (!builder.ContainsElement(friend))
            {
                builder.Add(link, friend);
                Search(strategyUser, builder, graph, result);
                builder.RemoveLast();
            }
        }
    }
}