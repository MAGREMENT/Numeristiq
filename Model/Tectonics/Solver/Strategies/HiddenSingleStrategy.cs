using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace Model.Tectonics.Solver.Strategies;

public class HiddenSingleStrategy : TectonicStrategy
{
    public HiddenSingleStrategy() : base("Hidden Single", StepDifficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }
    
    public override void Apply(ITectonicStrategyUser strategyUser)
    {
        for(int i = 0; i < strategyUser.Tectonic.Zones.Count; i++)
        {
            var zone = strategyUser.Tectonic.Zones[i];
            for (int n = 1; n <= zone.Count; n++)
            {
                var poss = strategyUser.ZonePositionsFor(i, n);
                if (poss.Count != 1) continue;

                strategyUser.ChangeBuffer.ProposeSolutionAddition(n, zone[poss.First(0, zone.Count)]);
                strategyUser.ChangeBuffer.Commit(DefaultChangeReportBuilder<IUpdatableTectonicSolvingState, ITectonicHighlighter>.Instance);
            }
        }
    }
}