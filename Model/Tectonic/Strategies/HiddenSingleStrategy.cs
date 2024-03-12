using System;
using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudoku.Solver;
using Model.Utility;

namespace Model.Tectonic.Strategies;

public class HiddenSingleStrategy : TectonicStrategy
{
    public HiddenSingleStrategy() : base("Hidden Single", StrategyDifficulty.Basic, OnCommitBehavior.WaitForAll)
    {
    }
    
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

public class HiddenSingleReportBuilder : IChangeReportBuilder<IUpdatableTectonicSolvingState,ITectonicHighlighter>
{
    public ChangeReport<ITectonicHighlighter> Build(IReadOnlyList<SolverProgress> changes, IUpdatableTectonicSolvingState snapshot)
    {
        throw new NotImplementedException();
    }
}