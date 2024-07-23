using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace Model.Tectonics.Solver.Strategies;

public class BruteForceStrategy : Strategy<ITectonicSolverData>
{
    private readonly TectonicBackTracker _backTracker = new()
    {
        StopAt = 1
    };
    
    public BruteForceStrategy() : base("Brute Force", StepDifficulty.ByTrial, InstanceHandling.FirstOnly)
    {
    }

    public override void Apply(ITectonicSolverData data)
    {
        _backTracker.Set(data.Tectonic.Copy(), data);
        if (!_backTracker.Fill()) return;

        for (int row = 0; row < data.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < data.Tectonic.ColumnCount; col++)
            {
                data.ChangeBuffer.ProposeSolutionAddition(_backTracker.Current[row, col], row, col);
            }
        }
        
        data.ChangeBuffer.Commit(DefaultNumericChangeReportBuilder<INumericSolvingState, ITectonicHighlighter>.Instance);
    }
}