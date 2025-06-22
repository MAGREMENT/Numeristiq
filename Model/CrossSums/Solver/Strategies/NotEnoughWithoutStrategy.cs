using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;

namespace Model.CrossSums.Solver.Strategies;

public class NotEnoughWithoutStrategy : Strategy<ICrossSumSolverData>
{
    public const string OfficialName = "Not Enough Without";
    
    public NotEnoughWithoutStrategy() : base(OfficialName, Difficulty.Easy, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(ICrossSumSolverData data)
    {
        for (int r = 0; r < data.CrossSum.RowCount; r++)
        {
            var remaining = data.GetRemainingForRow(r);
            if(remaining <= 0) continue;

            var available = data.GetAvailableForRow(r);
            for (int c = 0; c < data.CrossSum.ColumnCount; c++)
            {
                if(!data.IsAvailable(r, c)) continue;
                
                if(available - data.CrossSum[r, c] < remaining) 
                    data.ChangeBuffer.ProposePossibilityRemoval(r, c);
            }
            
            if (data.ChangeBuffer.NeedCommit())
            {
                data.ChangeBuffer.Commit(new NotEnoughWithoutReportBuilder());
                if (StopOnFirstCommit) return;
            }
        }
        
        for (int c = 0; c < data.CrossSum.ColumnCount; c++)
        {
            var remaining = data.GetRemainingForColumn(c);
            if(remaining <= 0) continue;

            var available = data.GetAvailableForColumn(c);
            for (int r = 0; r < data.CrossSum.RowCount; r++)
            {
                if(!data.IsAvailable(r, c)) continue;
                
                if(available - data.CrossSum[r, c] < remaining) 
                    data.ChangeBuffer.ProposePossibilityRemoval(r, c);
            }
            
            if (data.ChangeBuffer.NeedCommit())
            {
                data.ChangeBuffer.Commit(new NotEnoughWithoutReportBuilder());
                if (StopOnFirstCommit) return;
            }
        }
    }
}

public class NotEnoughWithoutReportBuilder : IChangeReportBuilder<DichotomousChange, IDichotomousSolvingState, ICrossSumHighlighter>
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