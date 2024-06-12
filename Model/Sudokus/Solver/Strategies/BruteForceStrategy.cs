using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies;

public class BruteForceStrategy : SudokuStrategy
{
    public const string OfficialName = "Brute Force";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    public BruteForceStrategy() : base(OfficialName, StepDifficulty.ByTrial, DefaultInstanceHandling) { }

    public override void Apply(ISudokuStrategyUser strategyUser)
    {
        var solution = BackTracking.Fill(strategyUser.Sudoku.Copy(), strategyUser, 1);

        if (solution.Count == 1) Process(strategyUser, solution[0]);
    }

    private void Process(ISudokuStrategyUser strategyUser, Sudoku s)
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if(strategyUser.Sudoku[r, c] != 0) continue;
                    
                strategyUser.ChangeBuffer.ProposeSolutionAddition(s[r, c], r, c);
            }
        }

        strategyUser.ChangeBuffer.Commit(DefaultChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>.Instance);
    }
}