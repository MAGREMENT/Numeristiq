using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace Model.Nonograms.Solver.Strategies;

public class BridgingStrategy : Strategy<INonogramSolverData>
{
    public BridgingStrategy() : base("Bridging", StepDifficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        for (int row = 0; row < data.Nonogram.RowCount; row++)
        {
            var space = data.PreComputer.HorizontalMainSpace(row);
            if (space.IsInvalid() || space.GetValueCount() != 1) continue;

            var last = -1;
            for (int c = space.Start; c <= space.End; c++)
            {
                if (!data.Nonogram[row, c]) continue;

                if (last >= 0)
                {
                    for (int i = last + 1; i < c; i++)
                    {
                        data.ChangeBuffer.ProposeSolutionAddition(row, i);
                    }

                    if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(
                            DefaultDichotomousChangeReportBuilder<IDichotomousSolvingState, INonogramHighlighter>
                                .Instance) && StopOnFirstPush) return;
                }

                last = c;
            }
        }
        
        for (int col = 0; col < data.Nonogram.ColumnCount; col++)
        {
            var space = data.PreComputer.VerticalMainSpace(col);
            if (space.IsInvalid() || space.GetValueCount() != 1) continue;

            var last = -1;
            for (int r = space.Start; r <= space.End; r++)
            {
                if (!data.Nonogram[r, col]) continue;

                if (last >= 0)
                {
                    for (int i = last + 1; i < r; i++)
                    {
                        data.ChangeBuffer.ProposeSolutionAddition(i, col);
                    }

                    if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(
                            DefaultDichotomousChangeReportBuilder<IDichotomousSolvingState, INonogramHighlighter>
                                .Instance) && StopOnFirstPush) return;
                }

                last = r;
            }
        }
    }
}