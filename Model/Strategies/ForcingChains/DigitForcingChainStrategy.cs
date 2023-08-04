using System.Collections.Generic;
using Model.StrategiesUtil;

namespace Model.Strategies.ForcingChains;

public class DigitForcingChainStrategy : IStrategy //TODO => fix for "4.21......5.....78...3......7..5...6......1.....4.........67.5.2.....4..3........"
{
    public string Name => "Digit forcing chain";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public int Score { get; set; }
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        LinkGraph<ILinkGraphElement> graph = strategyManager.LinkGraph();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in strategyManager.Possibilities[row, col])
                {
                    PossibilityCoordinate current = new PossibilityCoordinate(row, col, possibility);

                    Dictionary<ILinkGraphElement, Coloring> onColoring = new();
                    Dictionary<ILinkGraphElement, Coloring> offColoring = new();

                    onColoring[current] = Coloring.On;
                    offColoring[current] = Coloring.Off;
                    ForcingChainUtil.Color(graph, onColoring, current);
                    ForcingChainUtil.Color(graph, offColoring, current);

                    if(onColoring.Count == 1 || offColoring.Count == 1) continue;
                    Process(strategyManager, onColoring, offColoring);
                }
            }
        }
    }

    public string GetExplanation(IChangeCauseFactory factory)
    {
        return "";
    }

    private bool Process(IStrategyManager view, Dictionary<ILinkGraphElement, Coloring> onColoring,
        Dictionary<ILinkGraphElement, Coloring> offColoring)
    {
        bool wasProgressMade = false;
        foreach (var on in onColoring)
        {
            if (on.Key is not PossibilityCoordinate possOn) continue;
            if (offColoring.TryGetValue(possOn, out var other))
            {
                switch (other)
                {
                    case Coloring.Off when on.Value == Coloring.Off &&
                                           view.RemovePossibility(possOn.Possibility, possOn.Row, possOn.Col, this):
                    case Coloring.On when on.Value == Coloring.On &&
                                          view.AddDefinitiveNumber(possOn.Possibility, possOn.Row, possOn.Col, this):
                        wasProgressMade = true;
                        break;
                }
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

    private bool RemoveAll(IStrategyManager view, int row, int col, int except1, int except2)
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