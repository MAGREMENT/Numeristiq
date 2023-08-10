using System.Collections.Generic;
using Model.StrategiesUtil;

namespace Model.Strategies.ForcingNets;

public class UnitForcingNetStrategy : IStrategy
{
    public string Name => "Unit forcing net";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public int Score { get; set; }

    private readonly int _max;

    public UnitForcingNetStrategy(int maxPossibilities)
    {
        _max = maxPossibilities;
    }
    
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var ppir = strategyManager.PossibilityPositionsInRow(row, number);
                if (ppir.Count < 2 || ppir.Count > _max) continue;
                
                Dictionary<ILinkGraphElement, Coloring>[] colorings =
                    new Dictionary<ILinkGraphElement, Coloring>[ppir.Count];

                var cursor = 0;
                foreach (var col in ppir)
                {
                    colorings[cursor] = strategyManager.OnColoring(row, col, number);
                    cursor++;
                }
                
                Process(strategyManager, colorings);
                if (strategyManager.ChangeBuffer.NotEmpty())
                    strategyManager.ChangeBuffer.Push(this, new UnitForcingNetReportBuilder());
            }

            for (int col = 0; col < 9; col++)
            {
                var ppic = strategyManager.PossibilityPositionsInColumn(col, number);
                if (ppic.Count < 2 || ppic.Count > _max) continue;
                
                Dictionary<ILinkGraphElement, Coloring>[] colorings =
                    new Dictionary<ILinkGraphElement, Coloring>[ppic.Count];

                var cursor = 0;
                foreach (var row in ppic)
                {
                    colorings[cursor] = strategyManager.OnColoring(row, col, number);
                    cursor++;
                }
                
                Process(strategyManager, colorings);
                if (strategyManager.ChangeBuffer.NotEmpty())
                    strategyManager.ChangeBuffer.Push(this, new UnitForcingNetReportBuilder());
            }

            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var ppimn = strategyManager.PossibilityPositionsInMiniGrid(miniRow, miniCol, number);
                    if (ppimn.Count < 2 || ppimn.Count > _max) continue;
                
                    Dictionary<ILinkGraphElement, Coloring>[] colorings =
                        new Dictionary<ILinkGraphElement, Coloring>[ppimn.Count];

                    var cursor = 0;
                    foreach (var pos in ppimn)
                    {
                        colorings[cursor] = strategyManager.OnColoring(pos[0], pos[1], number);
                        cursor++;
                    }
                
                    Process(strategyManager, colorings);
                    if (strategyManager.ChangeBuffer.NotEmpty())
                        strategyManager.ChangeBuffer.Push(this, new UnitForcingNetReportBuilder());
                }
            }
        }
    }

    private void Process(IStrategyManager view, Dictionary<ILinkGraphElement, Coloring>[] colorings)
    {
        foreach (var element in colorings[0])
        {
            if (element.Key is not PossibilityCoordinate current) continue;

            bool sameInAll = true;
            Coloring col = element.Value;

            for (int i = 1; i < colorings.Length && sameInAll; i++)
            {
                if (!colorings[i].TryGetValue(current, out var c) || c != col) sameInAll = false;
            }

            if (sameInAll)
            {
                if (col == Coloring.On) view.ChangeBuffer.AddDefinitiveToAdd(current.Possibility, current.Row, current.Col);
                else view.ChangeBuffer.AddPossibilityToRemove(current.Possibility, current.Row, current.Col);
            }
        }
    }
}

public class UnitForcingNetReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes),
            lighter => IChangeReportBuilder.HighlightChanges(lighter, changes), "");
    }
}