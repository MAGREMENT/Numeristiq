using System.Collections.Generic;
using System.Linq;
using Model.StrategiesUtil;

namespace Model.Strategies.ForcingNets;

public class CellForcingNetStrategy : IStrategy
{
    public string Name => "Cell forcing net";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public int Score { get; set; }

    private readonly int _max;

    public CellForcingNetStrategy(int maxPossibilities)
    {
        _max = maxPossibilities;
    }
    
    public void ApplyOnce(IStrategyManager strategyManager)
    {
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
                    colorings[i] = strategyManager.OnColoring(row, col, possAsArray[i]);
                }

                Process(strategyManager, colorings);

                if (strategyManager.ChangeBuffer.NotEmpty())
                    strategyManager.ChangeBuffer.Push(this, new CellForcingNetReportBuilder());
            }
        }
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
                    view.ChangeBuffer.AddDefinitiveToAdd(current.Possibility, current.Row, current.Col);
                else view.ChangeBuffer.AddPossibilityToRemove(current.Possibility, current.Row, current.Col);
            }
        }
        
        //TODO type 3 and 4
    }
}

public class CellForcingNetReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes),
            lighter => IChangeReportBuilder.HighlightChanges(lighter, changes), "");
    }
}