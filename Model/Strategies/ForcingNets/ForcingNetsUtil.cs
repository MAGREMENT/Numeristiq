using System.Collections.Generic;
using Model.StrategiesUtil;

namespace Model.Strategies.ForcingChains;

public static class ForcingNetsUtil //TODO implement into precomputer
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
        else if (current is PossibilityCoordinate pos)
        {
            PossibilityCoordinate? row = null;
            bool rowB = true;
            PossibilityCoordinate? col = null;
            bool colB = true;
            PossibilityCoordinate? mini = null;
            bool miniB = true;
            
            foreach (var friend in graph.GetLinks(current, LinkStrength.Weak))
            {
                if (friend is not PossibilityCoordinate friendPos) continue;
                if (rowB && friendPos.Row == pos.Row)
                {
                    if (result.TryGetValue(friend, out var coloring))
                    {
                        if (coloring == Coloring.On) rowB = false;
                    }
                    else
                    {
                        if (row is null) row = friendPos;
                        else rowB = false;  
                    }
                    
                }

                if (colB && friendPos.Col == pos.Col)
                {
                    if (result.TryGetValue(friend, out var coloring))
                    {
                        if (coloring == Coloring.On) colB = false;
                    }
                    else
                    {
                        if (col is null) col = friendPos;
                        else colB = false;
                    }
                }

                if (miniB && friendPos.Row / 3 == pos.Row / 3 && friendPos.Col / 3 == pos.Col / 3)
                {
                    if (result.TryGetValue(friend, out var coloring))
                    {
                        if (coloring == Coloring.On) miniB = false;
                    }
                    else
                    {
                        if (mini is null) mini = friendPos;
                        else miniB = false;
                    }
                }
            }

            if (row is not null && rowB)
            {
                result[row] = Coloring.On;
                Color(graph, result, row);
            }

            if (col is not null && colB)
            {
                result[col] = Coloring.On;
                Color(graph, result, col);
            }

            if (mini is not null && miniB)
            {
                result[mini] = Coloring.On;
                Color(graph, result, mini);
            }
        }
    }
}