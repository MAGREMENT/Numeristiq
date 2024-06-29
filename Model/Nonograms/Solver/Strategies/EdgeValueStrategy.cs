using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace Model.Nonograms.Solver.Strategies;

public class EdgeValueStrategy : Strategy<INonogramSolverData>
{
    public EdgeValueStrategy() : base("Edge Value", StepDifficulty.Easy, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        for (int row = 0; row < data.Nonogram.RowCount; row++)
        {
            var space = data.PreComputer.HorizontalMainSpace(row);
            if (space.IsInvalid()) continue;

            if (data.Nonogram[row, space.Start])
            {
                var val = data.Nonogram.HorizontalLineCollection.TryGetValue(row, space.FirstValueIndex);
                for (int current = space.Start + 1; current < space.Start + val; current++)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(row, current);
                }

                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(DefaultDichotomousChangeReportBuilder<
                        IDichotomousSolvingState, INonogramHighlighter>.Instance) && StopOnFirstPush) return;
            }

            if (data.Nonogram[row, space.End])
            {
                var val = data.Nonogram.HorizontalLineCollection.TryGetValue(row, space.LastValueIndex);
                for (int current = space.End - 1; current > space.End - val; current--)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(row, current);
                }
                
                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(DefaultDichotomousChangeReportBuilder<
                        IDichotomousSolvingState, INonogramHighlighter>.Instance) && StopOnFirstPush) return;
            }
        }
        
        for (int col = 0; col < data.Nonogram.ColumnCount; col++)
        {
            var space = data.PreComputer.VerticalMainSpace(col);
            if (space.IsInvalid()) continue;

            if (data.Nonogram[space.Start, col])
            {
                var val = data.Nonogram.VerticalLineCollection.TryGetValue(col, space.FirstValueIndex);
                for (int current = space.Start + 1; current < space.Start + val; current++)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(current, col);
                }

                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(DefaultDichotomousChangeReportBuilder<
                        IDichotomousSolvingState, INonogramHighlighter>.Instance) && StopOnFirstPush) return;
            }

            if (data.Nonogram[space.End, col])
            {
                var val = data.Nonogram.VerticalLineCollection.TryGetValue(space.LastValueIndex, col);
                for (int current = space.End - 1; current > space.End - val; current--)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(current, col);
                }
                
                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(DefaultDichotomousChangeReportBuilder<
                        IDichotomousSolvingState, INonogramHighlighter>.Instance) && StopOnFirstPush) return;
            }
        }
    }
}