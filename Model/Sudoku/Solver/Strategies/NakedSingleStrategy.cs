using System.Collections.Generic;
using Model.Helpers.Changes;

namespace Model.Sudoku.Solver.Strategies;

public class NakedSingleStrategy : SudokuStrategy
{
    public const string OfficialName = "Naked Single";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public NakedSingleStrategy() : base(OfficialName, StrategyDifficulty.Basic, DefaultBehavior) {}
    
    public override void Apply(IStrategyUser strategyUser)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyUser.PossibilitiesAt(row, col).Count != 1) continue;
                
                strategyUser.ChangeBuffer.ProposeSolutionAddition(strategyUser.PossibilitiesAt(row, col).FirstPossibility(), row, col);
                strategyUser.ChangeBuffer.Commit( new NakedSingleReportBuilder());
                if (OnCommitBehavior == OnCommitBehavior.Return) return;
            }
        }
    }
}

public class NakedSingleReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport( Description(changes),
            lighter => IChangeReportBuilder.HighlightChanges(lighter, changes));
    }

    private static string Description(IReadOnlyList<SolverChange> changes)
    {
        return changes.Count != 1 ? "" : $"Naked Single in r{changes[0].Row + 1}c{changes[0].Column + 1}";
    }
}