using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;

namespace Model.Tectonics.Solver.Strategies;

public class BruteForceStrategy : Strategy<ITectonicSolverData>
{
    public BruteForceStrategy() : base("Brute Force", StepDifficulty.ByTrial, InstanceHandling.FirstOnly)
    {
    }

    public override void Apply(ITectonicSolverData data)
    {
        var solution = BackTracking.Solutions(data.Tectonic.Copy(), data, 1);
        if (solution.Count != 1) return;

        for (int row = 0; row < data.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < data.Tectonic.ColumnCount; col++)
            {
                data.ChangeBuffer.ProposeSolutionAddition(solution[0][row, col], row, col);
            }
        }
        
        data.ChangeBuffer.Commit(DefaultNumericChangeReportBuilder<IUpdatableTectonicSolvingState, ITectonicHighlighter>.Instance);
    }
}