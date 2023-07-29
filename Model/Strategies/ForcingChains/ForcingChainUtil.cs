using System.Collections.Generic;
using Model.StrategiesUtil;

namespace Model.Strategies.ForcingChains;

public static class ForcingChainUtil
{
    public static void Color(LinkGraph<ILinkGraphElement> graph, Dictionary<ILinkGraphElement, Coloring> result,
        ILinkGraphElement current)
    {
        Coloring opposite = result[current] == Coloring.On ? Coloring.Off : Coloring.On;

        foreach (var friend in graph.GetLinks(current, LinkStrength.Strong))
        {
            if (!result.ContainsKey(friend))
            {
                result[friend] = opposite;
                Color(graph, result, friend);
            }
        }

        if (opposite == Coloring.Off)
        {
            foreach (var friend in graph.GetLinks(current, LinkStrength.Weak))
            {
                if (!result.ContainsKey(friend))
                {
                    result[friend] = opposite;
                    Color(graph, result, friend);
                }
            }
        }
    }
}