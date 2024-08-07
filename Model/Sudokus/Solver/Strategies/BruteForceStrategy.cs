using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace Model.Sudokus.Solver.Strategies;

public class BruteForceStrategy : SudokuStrategy
{
    public const string OfficialName = "Brute Force";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    private readonly SudokuBackTracker _backTracker = new()
    {
        StopAt = 1
    };

    public BruteForceStrategy() : base(OfficialName, Difficulty.ByTrial, DefaultInstanceHandling) { }

    public override void Apply(ISudokuSolverData solverData)
    {
        _backTracker.Set(solverData.Sudoku.Copy(), solverData);
        if (!_backTracker.Fill()) return;
        
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if(solverData.Sudoku[r, c] != 0) continue;
                    
                solverData.ChangeBuffer.ProposeSolutionAddition(_backTracker.Current[r, c], r, c);
            }
        }

        if(solverData.ChangeBuffer.NeedCommit()) solverData.ChangeBuffer.Commit(
            DefaultNumericChangeReportBuilder<ISudokuSolvingState, ISudokuHighlighter>.Instance);
    }
}