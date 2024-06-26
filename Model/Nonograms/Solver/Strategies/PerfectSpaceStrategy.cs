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
            if (space.IsInvalid() || GetNeededSpace(data.Nonogram.HorizontalLineCollection.AsList(row),
                    space.ValueStart, space.ValueEnd) != space.End - space.Start + 1) continue;

            var cursor = space.Start;
            foreach (var v in data.Nonogram.HorizontalLineCollection[row])
            {
                var limit = cursor + v;
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
            if (space.IsInvalid() || GetNeededSpace(data.Nonogram.VerticalLineCollection.AsList(col),
                    space.ValueStart, space.ValueStart) != space.End - space.Start + 1) continue;

            var cursor = space.Start;
            foreach (var v in data.Nonogram.VerticalLineCollection[col])
            {
                var limit = cursor + v;
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

    private int GetNeededSpace(IReadOnlyList<int> values, int start, int end)
    {
        var result = 0;
        for (int i = start; i <= end; i++)
        {
            if (result > 0) result++;
            result += values[i];
        }

        return result;
    }

    
}

