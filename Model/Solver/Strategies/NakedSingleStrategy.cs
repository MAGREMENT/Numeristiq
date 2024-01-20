using System.Collections.Generic;
using System.Text;
using Model.Solver.Helpers.Changes;

namespace Model.Solver.Strategies;

public class NakedSingleStrategy : AbstractStrategy
{
    public const string OfficialName = "Naked Single";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public NakedSingleStrategy() : base(OfficialName, StrategyDifficulty.Basic, DefaultBehavior) {}
    
    public override void Apply(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.PossibilitiesAt(row, col).Count != 1) continue;
                
                strategyManager.ChangeBuffer.ProposeSolutionAddition(strategyManager.PossibilitiesAt(row, col).First(), row, col);
                strategyManager.ChangeBuffer.Commit(this, new NakedSingleReportBuilder());
                if (OnCommitBehavior == OnCommitBehavior.Return) return;
            }
        }
    }
}

public class NakedSingleReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Description(changes),
            lighter => IChangeReportBuilder.HighlightChanges(lighter, changes));
    }

    private static string Description(IReadOnlyList<SolverChange> changes)
    {
        return changes.Count != 1 ? "" : $"Naked Single in r{changes[0].Row + 1}c{changes[0].Column + 1}";
    }
}