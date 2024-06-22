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

    public override void Apply(ITectonicSolverData solverData)
    {
        var solution = BackTracking.Solutions(solverData.Tectonic.Copy(), solverData, 1);
        if (solution.Count != 1) return;

        for (int row = 0; row < solverData.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < solverData.Tectonic.ColumnCount; col++)
            {
                solverData.ChangeBuffer.ProposeSolutionAddition(solution[0][row, col], row, col);
            }
        }
        
        solverData.ChangeBuffer.Commit(DefaultNumericChangeReportBuilder<IUpdatableTectonicSolvingState, ITectonicHighlighter>.Instance);
    }
}