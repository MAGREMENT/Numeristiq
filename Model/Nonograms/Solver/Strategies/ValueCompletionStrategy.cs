using Model.Core;
using Model.Core.Changes;

namespace Model.Nonograms.Solver.Strategies;

public class ValueCompletionStrategy : Strategy<INonogramSolverData>
{
    public ValueCompletionStrategy() : base("Value Completion", StepDifficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        for (int row = 0; row < data.Nonogram.RowCount; row++)
        {
            foreach (var space in data.PreComputer.HorizontalValueSpaces(row))
            {
                if (space.GetLength() != space.Value) continue;

                if (space.Start > 0) data.ChangeBuffer.ProposePossibilityRemoval(row, space.Start - 1);
                if (space.End < data.Nonogram.ColumnCount - 1)
                    data.ChangeBuffer.ProposePossibilityRemoval(row, space.End + 1);

                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(DefaultDichotomousChangeReportBuilder<
                        IUpdatableDichotomousSolvingState, object>.Instance) && StopOnFirstPush) return;
            }
        }
        
        for (int col = 0; col < data.Nonogram.ColumnCount; col++)
        {
            foreach (var space in data.PreComputer.VerticalValueSpaces(col))
            {
                if (space.GetLength() != space.Value) continue;

                if (space.Start > 0) data.ChangeBuffer.ProposePossibilityRemoval(space.Start - 1, col);
                if (space.End < data.Nonogram.RowCount - 1)
                    data.ChangeBuffer.ProposePossibilityRemoval(space.End + 1, col);

                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(DefaultDichotomousChangeReportBuilder<
                        IUpdatableDichotomousSolvingState, object>.Instance) && StopOnFirstPush) return;
            }
        }
    }
}