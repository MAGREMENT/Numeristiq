using System.Collections.Generic;
using System.Linq;
using Model.StrategiesUtil;

namespace Model.Strategies.ForcingChains;

public class CellForcingChainStrategy : IStrategy
{
    public string Name => "Cell forcing chain";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public int Score { get; set; }

    private readonly int _max;

    public CellForcingChainStrategy(int maxPossibilities)
    {
        _max = maxPossibilities;
    }
    
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        var graph = strategyManager.LinkGraph();
        
        for (int row = 0; row < 9; row++)
        {
            for(int col = 0; col < 9; col++)
            {
                if (strategyManager.Possibilities[row, col].Count < 2|| strategyManager.Possibilities[row, col].Count > _max) continue;
                var possAsArray = strategyManager.Possibilities[row, col].ToArray();

                Dictionary<ILinkGraphElement, Coloring>[] colorings =
                    new Dictionary<ILinkGraphElement, Coloring>[possAsArray.Length];

                for (int i = 0; i < possAsArray.Length; i++)
                {
                    Dictionary<ILinkGraphElement, Coloring> currentColoring = new();
                    PossibilityCoordinate current = new PossibilityCoordinate(row, col, possAsArray[i]);

                    currentColoring[current] = Coloring.On;
                    ForcingChainUtil.Color(graph, currentColoring, current);
                    colorings[i] = currentColoring;
                }

                Process(strategyManager, colorings);
            }
        }
    }

    public string GetExplanation(IChangeCauseFactory factory)
    {
        return "";
    }

    private void Process(IStrategyManager view, Dictionary<ILinkGraphElement, Coloring>[] colorings)
    {
        foreach (var element in colorings[0])
        {
            if (element.Key is not PossibilityCoordinate current) continue;
            var currentColoring = element.Value;
            bool isSameInAll = true;

            for (int i = 1; i < colorings.Length && isSameInAll; i++)
            {
                if (!colorings[i].TryGetValue(current, out var c) || c != currentColoring) isSameInAll = false;
            }

            if (isSameInAll)
            {
                if (currentColoring == Coloring.On)
                    view.AddDefinitiveNumber(current.Possibility, current.Row, current.Col, this);
                else view.RemovePossibility(current.Possibility, current.Row, current.Col, this);
            }
        }
        
        //TODO type 3 and 4
    }
}