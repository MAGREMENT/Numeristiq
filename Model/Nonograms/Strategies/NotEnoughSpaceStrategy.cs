using Model.Core;
using Model.Core.Changes;

namespace Model.Nonograms.Strategies;

public class NotEnoughSpaceStrategy : Strategy<INonogramSolverData>
{
    public NotEnoughSpaceStrategy() : base("Not Enough Space", StepDifficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        for (int row = 0; row < data.Nonogram.RowCount; row++)
        {
            var list = data.Nonogram.HorizontalLineCollection.AsList(row);
            var cursor = 0;
            var start = -1;
            var col = 0;
            while(col < data.Nonogram.ColumnCount)
            {
                if (data.IsAvailable(row, col))
                {
                    col++;
                    continue;
                }
                
                if (data.Nonogram[row, col])
                {
                    start = -2;
                    cursor++;
                    if (cursor >= list.Count) break;
                    
                    do col++;
                    while (col < data.Nonogram.ColumnCount && data.Nonogram[row, col]);
                    
                    continue;
                }

                if (start != col - 1 && ProcessHorizontal(data, list[cursor], start + 1, col, row)) return;
                
                start = col;
                col++;
            }

            if (cursor < list.Count && ProcessHorizontal(data, list[cursor], start + 1, data.Nonogram.ColumnCount, row)) return;
        }
        
        for (int col = 0; col < data.Nonogram.RowCount; col++)
        {
            var list = data.Nonogram.VerticalLineCollection.AsList(col);
            var cursor = 0;
            var start = -1;
            var row = 0;
            while(row < data.Nonogram.RowCount)
            {
                if (data.IsAvailable(row, col))
                {
                    row++;
                    continue;
                }
                
                if (data.Nonogram[row, col])
                {
                    start = -2;
                    cursor++;
                    if (cursor >= list.Count) break;
                    
                    do row++;
                    while (row < data.Nonogram.RowCount && data.Nonogram[row, col]);
                    
                    continue;
                }

                if (start == row - 1)
                {
                    start = row;
                    row++;
                    continue;
                }

                if (ProcessVertical(data, list[cursor], start + 1, row, col)) return;
                row++;
            }

            if (cursor < list.Count && ProcessVertical(data, list[cursor], start + 1, data.Nonogram.RowCount, col)) return;
        }
    }

    private bool ProcessHorizontal(INonogramSolverData data, int val, int start, int current, int row)
    {
        if (start < 0 || current - start >= val) return false;

        for (int col = start; col < current; col++)
        {
            data.ChangeBuffer.ProposePossibilityRemoval(row, col);
        }

        return data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(DefaultDichotomousChangeReportBuilder<
            IUpdatableDichotomousSolvingState, object>.Instance) && StopOnFirstPush;
    }
    
    private bool ProcessVertical(INonogramSolverData data, int val, int start, int current, int col)
    {
        if (start < 0 || current - start >= val) return false;

        for (int row = start; row < current; row++)
        {
            data.ChangeBuffer.ProposePossibilityRemoval(row, col);
        }

        return data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(DefaultDichotomousChangeReportBuilder<
            IUpdatableDichotomousSolvingState, object>.Instance) && StopOnFirstPush;
    }
}