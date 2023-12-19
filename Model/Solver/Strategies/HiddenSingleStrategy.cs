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
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var u = i * 3 + j;
                    
                    var rp = strategyManager.RowPositionsAt(u, number);
                    if (rp.Count == 1) strategyManager.ChangeBuffer.ProposeSolutionAddition(number, u, rp.First());
                    
                    var cp = strategyManager.ColumnPositionsAt(u, number);
                    if (cp.Count == 1) strategyManager.ChangeBuffer.ProposeSolutionAddition(number, cp.First(), u);
                    
                    var mp = strategyManager.MiniGridPositionsAt(i, j, number);
                    if (mp.Count != 1) continue;
                    var pos = mp.First();
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