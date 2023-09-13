using System.Collections.Generic;
using System.Text;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;

namespace Model.Solver.Strategies;

/// <summary>
/// A naked single is a cell that contains only one possibility, therefore being the solution to that cell
/// </summary>
public class NakedSingleStrategy : IStrategy
{
    public string Name => "Naked single";
    public StrategyLevel Difficulty => StrategyLevel.Basic;
    public StatisticsTracker Tracker { get; } = new();
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.PossibilitiesAt(row, col).Count == 1) strategyManager.ChangeBuffer.AddDefinitiveToAdd(
                        strategyManager.PossibilitiesAt(row, col).GetFirst(), row, col);
            }
        }

        strategyManager.ChangeBuffer.Push(this, new NakedSingleReportBuilder());
    }
}

public class NakedSingleReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(changes),
            lighter => IChangeReportBuilder.HighlightChanges(lighter, changes));
    }

    private static string Explanation(List<SolverChange> changes)
    {
        var builder = new StringBuilder();

        foreach (var change in changes)
        {
            builder.Append($"{change.Number} is the solution to the cell [{change.Row + 1}, {change.Column + 1}]" +
                           " because it's the only possibility in that cell.\n");
        }

        return builder.ToString();
    }
}