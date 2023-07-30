using System.Collections.Generic;
using Model.StrategiesUtil;

namespace Model.Strategies.ForcingChains;

public class DigitForcingChainStrategy : IStrategy
{
    public string Name => "Digit forcing chain";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public int Score { get; set; }
    public void ApplyOnce(ISolverView solverView)
    {
        LinkGraph<ILinkGraphElement> graph = solverView.LinkGraph();
        HashSet<ILinkGraphElement> visited = new();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in solverView.Possibilities[row, col])
                {
                    PossibilityCoordinate current = new PossibilityCoordinate(row, col, possibility);
                    if (visited.Contains(current)) continue;

                    Dictionary<ILinkGraphElement, Coloring> onColoring = new();
                    Dictionary<ILinkGraphElement, Coloring> offColoring = new();

                    onColoring[current] = Coloring.On;
                    offColoring[current] = Coloring.Off;
                    ForcingChainUtil.Color(graph, onColoring, current);
                    ForcingChainUtil.Color(graph, offColoring, current);

                    if(onColoring.Count == 1 || offColoring.Count == 1) continue;
                    Process(solverView, onColoring, offColoring, visited);
                }
            }
        }
    }

    private bool Process(ISolverView view, Dictionary<ILinkGraphElement, Coloring> onColoring,
        Dictionary<ILinkGraphElement, Coloring> offColoring, HashSet<ILinkGraphElement> visited)
    {
        bool wasProgressMade = false;
        foreach (var on in onColoring)
        {
            if (on.Key is not PossibilityCoordinate possOn) continue;
            if (offColoring.TryGetValue(possOn, out var other))
            {
                if (other != on.Value) /*visited.Add(possOn)*/;
                else if (other == Coloring.On && on.Value == Coloring.On &&
                         view.AddDefinitiveNumber(possOn.Possibility, possOn.Row, possOn.Col, this))
                    wasProgressMade = true;
                else if(view.RemovePossibility(possOn.Possibility, possOn.Row, possOn.Col, this))
                    wasProgressMade = true;
            }

            if (on.Value != Coloring.On) continue;
            foreach (var off in offColoring)
            {
                if (off.Value != Coloring.On || off.Key is not PossibilityCoordinate possOff) continue;
                if (possOff.Row == possOn.Row && possOn.Col == possOff.Col &&
                    RemoveAll(view, possOn.Row, possOn.Col, possOn.Possibility, possOff.Possibility))
                    wasProgressMade = true;
                else if (possOff.Possibility == possOn.Possibility &&
                         possOn.ShareAUnit(possOff))
                {
                    foreach (var coord in possOn.SharedSeenCells(possOff))
                    {
                        if (view.RemovePossibility(possOn.Possibility, coord.Row, coord.Col, this))
                            wasProgressMade = true;
                    }
                }
            }
        }

        return wasProgressMade;
    }

    private bool RemoveAll(ISolverView view, int row, int col, int except1, int except2)
    {
        bool wasProgressMade = false;
        foreach (var possibility in view.Possibilities[row, col])
        {
            if (possibility == except1 || possibility == except2) continue;
            if (view.RemovePossibility(possibility, row, col, this)) wasProgressMade = true;
        }

        return wasProgressMade;
    }

    
}