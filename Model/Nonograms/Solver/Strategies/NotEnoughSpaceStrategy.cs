using Model.Core;
using Model.Core.Changes;

namespace Model.Nonograms.Solver.Strategies;

public class NotEnoughSpaceStrategy : Strategy<INonogramSolverData>
{
    public NotEnoughSpaceStrategy() : base("Not Enough Space", StepDifficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        for (int row = 0; row < data.Nonogram.RowCount; row++)
        {
            var min = data.Nonogram.HorizontalLineCollection.MinValue(row);
            int start = -1;
            for (int c = 0; c < data.Nonogram.ColumnCount; c++)
            {
                if (data.Nonogram[row, c])
                {
                    start = -2;
                    continue;
                }
                
                if (data.IsAvailable(row, c)) continue;

                if (TryProcessHorizontal(data, start, c, min, row)) return;

                start = c;
            }
            
            if (TryProcessHorizontal(data, start, data.Nonogram.ColumnCount, min, row)) return;
        }
        
        for (int col = 0; col < data.Nonogram.RowCount; col++)
        {
            var min = data.Nonogram.VerticalLineCollection.MinValue(col);
            int start = -1;
            for (int r = 0; r < data.Nonogram.ColumnCount; r++)
            {
                if (data.Nonogram[r, col])
                {
                    start = -2;
                    continue;
                }
                
                if (data.IsAvailable(r, col)) continue;

                if (TryProcessVertical(data, start, r, min, col)) return;

                start = r;
            }
            
            if (TryProcessVertical(data, start, data.Nonogram.ColumnCount, min, col)) return;
        }
    }

    private bool TryProcessHorizontal(INonogramSolverData data, int start, int current, int min, int row)
    {
        if (start == -2 || start == current - 1 || current - start - 1 >= min) return false;
        
        for (int i = start + 1; i < current; i++)
        {
            data.ChangeBuffer.ProposePossibilityRemoval(row, i);
        }

        return data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(
            DefaultDichotomousChangeReportBuilder<IUpdatableDichotomousSolvingState, object>
                .Instance) && StopOnFirstPush;
    }
    
    private bool TryProcessVertical(INonogramSolverData data, int start, int current, int min, int col)
    {
        if (start == -2 || start == current - 1 || current - start - 1 >= min) return false;
        
        for (int i = start + 1; i < current; i++)
        {
            data.ChangeBuffer.ProposePossibilityRemoval(i, col);
        }

        return data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(
            DefaultDichotomousChangeReportBuilder<IUpdatableDichotomousSolvingState, object>
                .Instance) && StopOnFirstPush;
    }
}