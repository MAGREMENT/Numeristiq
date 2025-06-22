using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;

namespace Model.CrossSums.Solver.Strategies;

public class TooBigStrategy : Strategy<ICrossSumSolverData>
{
    public const string OfficialName = "Too Big";
    
    public TooBigStrategy() : base(OfficialName, Difficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(ICrossSumSolverData data)
    {
        for (int r = 0; r < data.CrossSum.RowCount; r++)
        {
            var remaining = data.GetRemainingForRow(r);
            if(remaining <= 0) continue;
            
            for (int c = 0; c < data.CrossSum.ColumnCount; c++)
            {
                if(!data.IsAvailable(r, c)) continue;

                var v = data.CrossSum[r, c];
                if (v > remaining)
                {
                    data.ChangeBuffer.ProposePossibilityRemoval(r, c);
                    if (data.ChangeBuffer.NeedCommit())
                    {
                        data.ChangeBuffer.Commit(new TooBigReportBuilder());
                        if(StopOnFirstCommit) return;
                    }
                    
                    continue;
                }

                remaining = data.GetRemainingForColumn(c);
                if (v > remaining)
                {
                    data.ChangeBuffer.ProposePossibilityRemoval(r, c);
                    if (data.ChangeBuffer.NeedCommit())
                    {
                        data.ChangeBuffer.Commit(new TooBigReportBuilder());
                        if(StopOnFirstCommit) return;
                    }
                }
            }
        }
    }
}

public class TooBigReportBuilder : IChangeReportBuilder<DichotomousChange, IDichotomousSolvingState, ICrossSumHighlighter>
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