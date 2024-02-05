using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Sudoku.Solver;
using Model.Utility;

namespace Model.Tectonic.Strategies;

public class HiddenSingleStrategy : AbstractStrategy
{
    public override void Apply(IStrategyUser strategyUser)
    {
        for(int i = 0; i < strategyUser.Tectonic.Zones.Count; i++)
        {
            for (int n = 1; n <= strategyUser.Tectonic.Zones[i].Count; n++)
            {
                var poss = strategyUser.ZonePositionsFor(i, n);
                if (poss.Count != 1) continue;

                strategyUser.ChangeBuffer.ProposeSolutionAddition(n, strategyUser.Tectonic.Zones[i][poss.First(0, strategyUser.Tectonic.Zones[i].Count)]);
                strategyUser.ChangeBuffer.Commit(new HiddenSingleReportBuilder());
            }
        }
    }
}

public class HiddenSingleReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return ChangeReport.Default(changes);
    }
}