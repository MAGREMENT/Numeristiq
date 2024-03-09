using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.StrategiesUtility;

namespace Model.Sudoku.Solver.Strategies;

public class BruteForceStrategy : SudokuStrategy
{
    public const string OfficialName = "Brute Force";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public BruteForceStrategy() : base(OfficialName, StrategyDifficulty.ByTrial, DefaultBehavior) { }

    public override void Apply(IStrategyUser strategyUser)
    {
        var solution = BackTracking.Fill(strategyUser.Sudoku.Copy(), strategyUser, 1);

        if (solution.Length == 1) Process(strategyUser, solution[0]);
    }

    private void Process(IStrategyUser strategyUser, Sudoku s)
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if(strategyUser.Sudoku[r, c] != 0) continue;
                    
                strategyUser.ChangeBuffer.ProposeSolutionAddition(s[r, c], r, c);
            }
        }

        strategyUser.ChangeBuffer.Commit(new BruteForceReportBuilder());
    }
}

public class BruteForceReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState>
{
    public ChangeReport Build(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return ChangeReport.Default(changes);
    }
}