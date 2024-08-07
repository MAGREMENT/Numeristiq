using Model.Core;
using Model.Core.BackTracking;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Nonograms.BackTrackers;

namespace Model.Nonograms.Solver.Strategies;

public class BruteForceStrategy : Strategy<INonogramSolverData>
{
    private readonly BackTracker<Nonogram, IAvailabilityChecker> _backTracker
        = new NaiveNonogramBackTracker();
    
    public BruteForceStrategy() : base("Brute Force", Difficulty.ByTrial, InstanceHandling.FirstOnly)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        //Too slow above this limit
        if (data.Nonogram.RowCount + data.Nonogram.ColumnCount > 20) return;
        
        _backTracker.Set(data.Nonogram.Copy(), data);
        if (!_backTracker.Fill()) return;

        for (int row = 0; row < data.Nonogram.RowCount; row++)
        {
            for (int col = 0; col < data.Nonogram.ColumnCount; col++)
            {
                if (_backTracker.Current[row, col]) data.ChangeBuffer.ProposeSolutionAddition(row, col);
            }
        }

        if(data.ChangeBuffer.NeedCommit()) data.ChangeBuffer.Commit(
            DefaultDichotomousChangeReportBuilder<INonogramSolvingState, INonogramHighlighter>.Instance);
    }
}