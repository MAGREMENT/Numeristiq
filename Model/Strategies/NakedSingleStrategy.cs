using System.Collections.Generic;
using Model.Changes;
using Model.Solver;

namespace Model.Strategies;

public class NakedSingleStrategy : IStrategy
{
    public string Name => "Naked single";
    public StrategyLevel Difficulty => StrategyLevel.Basic;
    public int Score { get; set; }
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Possibilities[row, col].Count == 1) strategyManager.ChangeBuffer.AddDefinitiveToAdd(
                        strategyManager.Possibilities[row, col].GetFirst(), row, col);
            }
        }

        strategyManager.ChangeBuffer.Push(this, new NakedSingleReportBuilder());
    }
}

public class NakedSingleReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes),
            "The numbers were added for being the only one in their cell",
            lighter => IChangeReportBuilder.HighlightChanges(lighter, changes));
    }
}