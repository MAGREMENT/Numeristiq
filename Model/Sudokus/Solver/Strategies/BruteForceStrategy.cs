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

    public override void Apply(ISudokuSolverData solverData)
    {
        var solution = BackTracking.Solutions(solverData.Sudoku.Copy(), solverData, 1);

        if (solution.Count == 1) Process(solverData, solution[0]);
    }

    private void Process(ISudokuSolverData solverData, Sudoku s)
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if(solverData.Sudoku[r, c] != 0) continue;
                    
                solverData.ChangeBuffer.ProposeSolutionAddition(s[r, c], r, c);
            }
        }

        solverData.ChangeBuffer.Commit(DefaultChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>.Instance);
    }
}