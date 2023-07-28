using System.Collections.Generic;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LoopFinder;

namespace Model.Strategies.ForcingChains;

public class DigitForcingChainStrategy : IStrategy
{
    public string Name => "Digit forcing chain";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public int Score { get; set; }
    public void ApplyOnce(ISolverView solverView)
    {
        Graph<PossibilityCoordinate> graph = solverView.LinkGraph();
        HashSet<PossibilityCoordinate> visited = new();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in solverView.Possibilities[row, col])
                {
                    PossibilityCoordinate current = new PossibilityCoordinate(row, col, possibility);
                    if (visited.Contains(current)) continue;

                    Dictionary<PossibilityCoordinate, Coloring> onColoring = new();
                    Dictionary<PossibilityCoordinate, Coloring> offColoring = new();

                    onColoring[current] = Coloring.On;
                    offColoring[current] = Coloring.Off;
                    Color(graph, onColoring, current);
                    Color(graph, offColoring, current);

                    if(onColoring.Count == 1 || offColoring.Count == 1) continue;
                    if (Process(solverView, onColoring, offColoring, visited)) /*return*/;
                }
            }
        }
    }

    private bool Process(ISolverView view, Dictionary<PossibilityCoordinate, Coloring> onColoring,
        Dictionary<PossibilityCoordinate, Coloring> offColoring, HashSet<PossibilityCoordinate> visited)
    {
        bool wasProgressMade = false;
        foreach (var possOn in onColoring)
        {
            if (offColoring.TryGetValue(possOn.Key, out var other))
            {
                if (other != possOn.Value) visited.Add(possOn.Key);
                else if (other == Coloring.On && possOn.Value == Coloring.On &&
                         view.AddDefinitiveNumber(possOn.Key.Possibility, possOn.Key.Row, possOn.Key.Col, this))
                    wasProgressMade = true;
                else if(view.RemovePossibility(possOn.Key.Possibility, possOn.Key.Row, possOn.Key.Col, this))
                    wasProgressMade = true;
            }

            if (possOn.Value != Coloring.On) continue;
            foreach (var possOff in offColoring)
            {
                if (possOff.Value != Coloring.On) continue;
                if (possOff.Key.Row == possOn.Key.Row && possOn.Key.Col == possOff.Key.Col &&
                    RemoveAll(view, possOn.Key.Row, possOn.Key.Col, possOn.Key.Possibility, possOff.Key.Possibility))
                    wasProgressMade = true;
                else if (possOff.Key.Possibility == possOn.Key.Possibility &&
                         possOn.Key.ShareAUnit(possOff.Key))
                {
                    foreach (var coord in possOn.Key.SharedSeenCells(possOff.Key))
                    {
                        if (view.RemovePossibility(possOn.Key.Possibility, coord.Row, coord.Col, this))
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

    private void Color(Graph<PossibilityCoordinate> graph, Dictionary<PossibilityCoordinate, Coloring> result,
        PossibilityCoordinate current)
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