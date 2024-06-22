using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace Model.Tectonics.Solver.Strategies;

public class HiddenSingleStrategy : Strategy<ITectonicSolverData>
{
    public HiddenSingleStrategy() : base("Hidden Single", StepDifficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }
    
    public override void Apply(ITectonicSolverData solverData)
    {
        for(int i = 0; i < solverData.Tectonic.Zones.Count; i++)
        {
            var zone = solverData.Tectonic.Zones[i];
            for (int n = 1; n <= zone.Count; n++)
            {
                var poss = solverData.ZonePositionsFor(i, n);
                if (poss.Count != 1) continue;

                solverData.ChangeBuffer.ProposeSolutionAddition(n, zone[poss.First(0, zone.Count)]);
                solverData.ChangeBuffer.Commit(DefaultNumericChangeReportBuilder<IUpdatableTectonicSolvingState, ITectonicHighlighter>.Instance);
            }
        }
    }
}