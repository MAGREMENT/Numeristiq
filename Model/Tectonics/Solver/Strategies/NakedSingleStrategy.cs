using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace Model.Tectonics.Solver.Strategies;

public class NakedSingleStrategy : Strategy<ITectonicSolverData>
{
    public NakedSingleStrategy() : base("Naked Single", StepDifficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }
    
    public override void Apply(ITectonicSolverData data)
    {
        for (int row = 0; row < data.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < data.Tectonic.ColumnCount; col++)
            {
                var p = data.PossibilitiesAt(row, col);
                if (p.Count != 1) continue;
            
                data.ChangeBuffer.ProposeSolutionAddition(
                    p.First(1, data.Tectonic.GetZone(row, col).Count), row, col);
                data.ChangeBuffer.Commit(DefaultNumericChangeReportBuilder<INumericSolvingState, ITectonicHighlighter>.Instance);
            }
        }
    }
}