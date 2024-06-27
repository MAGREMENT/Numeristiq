using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;

namespace Model.Nonograms.Solver.Strategies;

public class PerfectSpaceStrategy : Strategy<INonogramSolverData>
{
    public PerfectSpaceStrategy() : base("Perfect Space", StepDifficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        for (int row = 0; row < data.Nonogram.RowCount; row++)
        {
            var space = data.PreComputer.HorizontalMainSpace(row);
            if (space.IsInvalid() || data.Nonogram.HorizontalLineCollection.NeededSpace(row,
                    space.FirstValueIndex, space.LastValueIndex) != space.End - space.Start + 1) continue;

            var cursor = space.Start;
            for(int index = space.FirstValueIndex; index <= space.LastValueIndex; index++)
            {
                var limit = cursor + data.Nonogram.HorizontalLineCollection.TryGetValue(row, index);
                for (; cursor < limit; cursor++)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(row, cursor);
                }

                cursor++;
            }

            if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(DefaultDichotomousChangeReportBuilder<
                    IUpdatableDichotomousSolvingState, object>.Instance) && StopOnFirstPush) return;
        }
        
        for (int col = 0; col < data.Nonogram.ColumnCount; col++)
        {
            var space = data.PreComputer.VerticalMainSpace(col);
            if (space.IsInvalid() || data.Nonogram.VerticalLineCollection.NeededSpace(col,
                    space.FirstValueIndex, space.LastValueIndex) != space.End - space.Start + 1) continue;

            var cursor = space.Start;
            for(int index = space.FirstValueIndex; index <= space.LastValueIndex; index++)
            {
                var limit = cursor + data.Nonogram.VerticalLineCollection.TryGetValue(col, index);
                for (; cursor < limit; cursor++)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(cursor, col);
                }

                cursor++;
            }
            
            if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(DefaultDichotomousChangeReportBuilder<
                    IUpdatableDichotomousSolvingState, object>.Instance) && StopOnFirstPush) return;
        }
    }
}

