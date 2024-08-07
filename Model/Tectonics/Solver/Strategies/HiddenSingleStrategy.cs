using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace Model.Tectonics.Solver.Strategies;

public class HiddenSingleStrategy : Strategy<ITectonicSolverData>
{
    public HiddenSingleStrategy() : base("Hidden Single", Difficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }
    
    public override void Apply(ITectonicSolverData data)
    {
        for(int i = 0; i < data.Tectonic.Zones.Count; i++)
        {
            var zone = data.Tectonic.Zones[i];
            for (int n = 1; n <= zone.Count; n++)
            {
                var poss = data.ZonePositionsFor(i, n);
                if (poss.Count != 1) continue;

                data.ChangeBuffer.ProposeSolutionAddition(n, zone[poss.First(0, zone.Count)]);
                if (data.ChangeBuffer.NeedCommit())
                {
                    data.ChangeBuffer.Commit(DefaultNumericChangeReportBuilder<INumericSolvingState, ITectonicHighlighter>.Instance);
                    if (StopOnFirstCommit) return;
                }
            }
        }
    }
}