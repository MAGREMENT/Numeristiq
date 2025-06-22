using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;

namespace Model.CrossSums.Solver.Strategies;

public class PerfectCountStrategy : Strategy<ICrossSumSolverData>
{
    public const string OfficialName = "Perfect Count";
    
    public PerfectCountStrategy() : base(OfficialName, Difficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(ICrossSumSolverData data)
    {
        for (int r = 0; r < data.CrossSum.RowCount; r++)
        {
            var remaining = data.GetRemainingForRow(r);
            if(remaining <= 0) continue;

            var n = data.GetAvailableForRow(r);
            if (remaining != n) continue;

            for (int c = 0; c < data.CrossSum.ColumnCount; c++)
            {
                if(data.IsAvailable(r,c)) data.ChangeBuffer.ProposeSolutionAddition(r, c);
            }

            if (data.ChangeBuffer.NeedCommit())
            {
                data.ChangeBuffer.Commit(new PerfectCountReportBuilder());
                if (StopOnFirstCommit) return;
            }
        }
        
        for (int c = 0; c < data.CrossSum.ColumnCount; c++)
        {
            var remaining = data.GetRemainingForColumn(c);
            if(remaining <= 0) continue;

            var n = data.GetAvailableForColumn(c);
            if (remaining != n) continue;

            for (int r = 0; r < data.CrossSum.RowCount; r++)
            {
                if(data.IsAvailable(r,c)) data.ChangeBuffer.ProposeSolutionAddition(r, c);
            }

            if (data.ChangeBuffer.NeedCommit())
            {
                data.ChangeBuffer.Commit(new PerfectCountReportBuilder());
                if (StopOnFirstCommit) return;
            }
        }
    }
}

public class PerfectCountReportBuilder : IChangeReportBuilder<DichotomousChange, IDichotomousSolvingState, ICrossSumHighlighter>
{
    public ChangeReport<ICrossSumHighlighter> BuildReport(IReadOnlyList<DichotomousChange> changes, IDichotomousSolvingState snapshot)
    {
        throw new System.NotImplementedException();
    }

    public Clue<ICrossSumHighlighter> BuildClue(IReadOnlyList<DichotomousChange> changes, IDichotomousSolvingState snapshot)
    {
        throw new System.NotImplementedException();
    }
}