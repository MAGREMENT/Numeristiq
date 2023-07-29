using System.Collections.Generic;
using Model.StrategiesUtil;

namespace Model.Strategies.ForcingChains;

public class NishioForcingChainStrategy : IStrategy
{
    public string Name => "Nishio forcing chain";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public int Score { get; set; }
    public void ApplyOnce(ISolverView solverView)
    {
        LinkGraph<ILinkGraphElement> graph = solverView.LinkGraph();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in solverView.Possibilities[row, col])
                {
                    PossibilityCoordinate current = new PossibilityCoordinate(row, col, possibility);

                    Dictionary<ILinkGraphElement, Coloring> coloring = new();
                    coloring[current] = Coloring.On;

                    if (Search(solverView, graph, coloring, current))
                    {
                        solverView.RemovePossibility(possibility, row, col, this);
                        return;
                    }
                }
            }
        }
    }
    
    private bool Search(ISolverView view, LinkGraph<ILinkGraphElement> graph, Dictionary<ILinkGraphElement, Coloring> result,
        ILinkGraphElement current) //TODO other types
    {
        Coloring opposite = result[current] == Coloring.On ? Coloring.Off : Coloring.On;

        foreach (var friend in graph.GetLinks(current, LinkStrength.Strong))
        {
            if (!result.ContainsKey(friend))
            {
                result[friend] = opposite;
                if (Search(view, graph, result, friend)) return true;
            }
            else if (result[friend] != opposite) return true;
        }

        if (opposite == Coloring.Off)
        {
            foreach (var friend in graph.GetLinks(current, LinkStrength.Weak))
            {
                if (!result.ContainsKey(friend))
                {
                    result[friend] = opposite;
                    if (Search(view, graph, result, friend)) return true;
                }
                else if (result[friend] != opposite) return true;
            }
        }

        return false;
    }
}