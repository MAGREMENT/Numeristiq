using System.Collections.Generic;
using Global.Enums;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.StrategiesUtility.Oddagons.Algorithms;

public class OddagonSearchAlgorithmV2 : IOddagonSearchAlgorithm
{
    private readonly int _maxLength;

    public OddagonSearchAlgorithmV2(int maxLength)
    {
        _maxLength = maxLength;
    }


    public List<AlmostOddagon> Search(IStrategyManager strategyManager, LinkGraph<CellPossibility> graph)
    {
        List<AlmostOddagon> result = new();
        foreach (var start in graph)
        {
            Search(strategyManager, new LinkGraphChainBuilder<CellPossibility>(start), graph, result);
        }

        return result;
    }

    private void Search(IStrategyManager strategyManager, LinkGraphChainBuilder<CellPossibility> builder,
        LinkGraph<CellPossibility> graph, List<AlmostOddagon> result)
    {
        if (builder.Count > _maxLength) return;
        
        var last = builder.LastElement();
        var first = builder.FirstElement();

        foreach (var friend in graph.GetLinks(last))
        {
            var link = graph.HasLinkTo(last, friend, LinkStrength.Strong) ? LinkStrength.Strong : LinkStrength.Weak;

            if (friend == first)
            {
                if (builder.Count < 5 || builder.Count % 2 != 1) continue;

                result.Add(AlmostOddagon.FromBoard(strategyManager, builder.ToLoop(link)));
            }
            else if (!builder.ContainsElement(friend))
            {
                builder.Add(link, friend);
                Search(strategyManager, builder, graph, result);
                builder.RemoveLast();
            }
        }
    }
}