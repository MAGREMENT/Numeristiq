using System;
using System.Collections.Generic;
using System.Text;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;

namespace Model.Solver.Strategies;

public class HiddenSingleStrategy : AbstractStrategy
{
    public const string OfficialName = "Hidden Single";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public HiddenSingleStrategy() : base(OfficialName, StrategyDifficulty.Basic, DefaultBehavior){}
    
    public override void Apply(IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var ppir = strategyManager.RowPositionsAt(row, number);
                if (ppir.Count == 1) strategyManager.ChangeBuffer.ProposeSolutionAddition(number, row, ppir.First());
            }

            for (int col = 0; col < 9; col++)
            {
                var ppic = strategyManager.ColumnPositionsAt(col, number);
                if (ppic.Count == 1) strategyManager.ChangeBuffer.ProposeSolutionAddition(number, ppic.First(), col);
            }

            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var ppimn = strategyManager.MiniGridPositionsAt(miniRow, miniCol, number);
                    if (ppimn.Count != 1) continue;
                    
                    var pos = ppimn.First();
                    strategyManager.ChangeBuffer.ProposeSolutionAddition(number, pos.Row, pos.Column);
                }
            }
        }

        strategyManager.ChangeBuffer.Commit(this, new HiddenSingleReportBuilder());
    }
}

public class HiddenSingleReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(changes, snapshot),
            lighter => IChangeReportBuilder.HighlightChanges(lighter, changes));
    }

    private static string Explanation(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        var builder = new StringBuilder();

        foreach (var change in changes)
        {
            string where;
            if (snapshot.RowPositionsAt(change.Row, change.Number).Count == 1)
                where = $"row {change.Row + 1}";
            else if (snapshot.ColumnPositionsAt(change.Column, change.Number).Count == 1)
                where = $"column {change.Column + 1}";
            else
            {
                var miniGridPositions = snapshot.MiniGridPositionsAt(change.Row / 3, change.Column / 3, change.Number);
                if (miniGridPositions.Count == 1) where = $"mini grid {miniGridPositions.MiniGridNumber() + 1}";
                else throw new Exception("Error while backtracking hidden singles");
            }

            builder.Append($"{change.Number} is the solution to the cell [{change.Row + 1}, {change.Column + 1}]" +
                           $" because it's the only cell with that possibility in {where}.\n");
        }

        return builder.ToString();
    }

}