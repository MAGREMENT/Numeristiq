using Model.Core;
using Model.Core.Changes;
using Model.Kakuros;
using Model.Utility;

namespace Model.Nonograms.Strategies;

public class PerfectSpaceStrategy : Strategy<INonogramSolverData>
{
    public PerfectSpaceStrategy() : base("Perfect Space", StepDifficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        for (int row = 0; row < data.Nonogram.RowCount; row++)
        {
            var spaces = data.HorizontalSpacesFor(row);
            if (spaces.Count != 1 || data.Nonogram.HorizontalLineCollection.SpaceNeeded(row) != spaces[0].GetLength) continue;

            int cursor = spaces[0].Start;
            foreach (var val in data.Nonogram.HorizontalLineCollection[row])
            {
                var limit = cursor + val;
                for (; cursor < limit; cursor++)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(new Cell(row, cursor));
                }
                
                cursor++;
            }

            if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(DefaultDichotomousChangeReportBuilder<
                    IUpdatableDichotomousSolvingState, object>.Instance) && StopOnFirstPush) return;
        }
        
        for (int col = 0; col < data.Nonogram.ColumnCount; col++)
        {
            var spaces = data.VerticalSpacesFor(col);
            if (spaces.Count != 1 || data.Nonogram.VerticalLineCollection.SpaceNeeded(col) != spaces[0].GetLength) continue;

            int cursor = spaces[0].Start;
            foreach (var val in data.Nonogram.VerticalLineCollection[col])
            {
                var limit = cursor + val;
                for (; cursor < limit; cursor++)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(new Cell(cursor, col));
                }
                
                cursor++;
            }

            if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(DefaultDichotomousChangeReportBuilder<
                    IUpdatableDichotomousSolvingState, object>.Instance) && StopOnFirstPush) return;
        }
    }
}