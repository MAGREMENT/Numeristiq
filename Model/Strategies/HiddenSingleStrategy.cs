using System.Collections.Generic;
using System.Linq;

namespace Model.Strategies;

public class HiddenSingleStrategy : IStrategy
{
    public string Name => "Hidden single";
    public StrategyLevel Difficulty => StrategyLevel.Basic;
    public int Score { get; set; }
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var ppir = strategyManager.PossibilityPositionsInRow(row, number);
                if (ppir.Count == 1) strategyManager.ChangeBuffer.AddDefinitiveToAdd(number, row, ppir.First());
            }

            for (int col = 0; col < 9; col++)
            {
                var ppic = strategyManager.PossibilityPositionsInColumn(col, number);
                if (ppic.Count == 1) strategyManager.ChangeBuffer.AddDefinitiveToAdd(number, ppic.First(), col);
            }

            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var ppimn = strategyManager.PossibilityPositionsInMiniGrid(miniRow, miniCol, number);
                    if (ppimn.Count == 1)
                    {
                        var pos = ppimn.First();
                        strategyManager.ChangeBuffer.AddDefinitiveToAdd(number, pos[0], pos[1]);
                    }
                }
            }
        }

        strategyManager.ChangeBuffer.Push(this, new HiddenSingleReportBuilder());
    }
}

public class HiddenSingleReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes),
            lighter => IChangeReportBuilder.HighlightChanges(lighter, changes),
            "The numbers were added for being the only one in their unit");
    }
}