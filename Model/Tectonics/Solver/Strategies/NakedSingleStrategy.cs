using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace Model.Tectonics.Solver.Strategies;

public class NakedSingleStrategy : Strategy<ITectonicSolverData>
{
    public NakedSingleStrategy() : base("Naked Single", StepDifficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }
    
    public override void Apply(ITectonicSolverData solverData)
    {
        for (int row = 0; row < solverData.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < solverData.Tectonic.ColumnCount; col++)
            {
                var p = solverData.PossibilitiesAt(row, col);
                if (p.Count != 1) continue;
            
                solverData.ChangeBuffer.ProposeSolutionAddition(
                    p.First(1, solverData.Tectonic.GetZone(row, col).Count), row, col);
                solverData.ChangeBuffer.Commit(DefaultNumericChangeReportBuilder<IUpdatableTectonicSolvingState, ITectonicHighlighter>.Instance);
            }
        }
    }
}