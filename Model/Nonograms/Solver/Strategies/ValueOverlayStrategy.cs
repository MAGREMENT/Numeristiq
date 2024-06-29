using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace Model.Nonograms.Solver.Strategies;

public class ValueOverlayStrategy : Strategy<INonogramSolverData>
{
    public ValueOverlayStrategy() : base("Value Overlay", StepDifficulty.Medium, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        for (int row = 0; row < data.Nonogram.RowCount; row++)
        {
            foreach (var space in data.PreComputer.HorizontalValueSpaces(row))
            {
                for (int col = space.End - space.Value + 1; col < space.Start + space.Value; col++)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(row, col);
                }

                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(DefaultDichotomousChangeReportBuilder<
                        IDichotomousSolvingState, INonogramHighlighter>.Instance) && StopOnFirstPush) return;
            }
        }
        
        for (int col = 0; col < data.Nonogram.ColumnCount; col++)
        {
            foreach (var space in data.PreComputer.VerticalValueSpaces(col))
            {
                for (int row = space.End - space.Value + 1; row < space.Start + space.Value; row++)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(row, col);
                }

                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(DefaultDichotomousChangeReportBuilder<
                        IDichotomousSolvingState, INonogramHighlighter>.Instance) && StopOnFirstPush) return;
            }
        }
    }
}