using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;

namespace Model.CrossSums.Solver.Strategies;

public class SingleOddStrategy : Strategy<ICrossSumSolverData>
{
    public const string OfficialName = "Single Odd";
    
    public SingleOddStrategy() : base(OfficialName, Difficulty.Easy, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(ICrossSumSolverData data)
    {
        for (int r = 0; r < data.CrossSum.RowCount; r++)
        {
            var remaining = data.GetRemainingForRow(r);
            if(remaining <= 0) continue;

            var ind = -1;
            for (int c = 0; c < data.CrossSum.ColumnCount; c++)
            {
                if (!data.IsAvailable(r, c) || data.CrossSum[r, c] % 2 == 0) continue;

                if (ind == -1) ind = c;
                else
                {
                    ind = -1;
                    break;
                }
            }
            
            if(ind == -1) continue;
            
            if(remaining % 2 == 1) data.ChangeBuffer.ProposeSolutionAddition(r, ind);
            else data.ChangeBuffer.ProposePossibilityRemoval(r, ind);

            if (data.ChangeBuffer.NeedCommit())
            {
                data.ChangeBuffer.Commit(new SingleOddReportBuilder());
                if(StopOnFirstCommit) return;
            }
        }
        
        for (int c = 0; c < data.CrossSum.ColumnCount; c++)
        {
            var remaining = data.GetRemainingForColumn(c);
            if(remaining <= 0) continue;

            var ind = -1;
            for (int r = 0; r < data.CrossSum.RowCount; r++)
            {
                if (!data.IsAvailable(r, c) || data.CrossSum[r, c] % 2 == 0) continue;

                if (ind == -1) ind = r;
                else
                {
                    ind = -1;
                    break;
                }
            }
            
            if(ind == -1) continue;
            
            if(remaining % 2 == 1) data.ChangeBuffer.ProposeSolutionAddition(ind, c);
            else data.ChangeBuffer.ProposePossibilityRemoval(ind, c);

            if (data.ChangeBuffer.NeedCommit())
            {
                data.ChangeBuffer.Commit(new SingleOddReportBuilder());
                if(StopOnFirstCommit) return;
            }
        }
    }
}

public class SingleOddReportBuilder : IChangeReportBuilder<DichotomousChange, IDichotomousSolvingState, ICrossSumHighlighter>
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