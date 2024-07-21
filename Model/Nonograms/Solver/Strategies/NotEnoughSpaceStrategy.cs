using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;

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
            int c, e, min, ind;
            var main = data.PreComputer.HorizontalRemainingValuesSpace(row);
            if (main.IsInvalid())
            {
                c = 0;
                e = data.Nonogram.ColumnCount - 1;
                (ind, min) = data.Nonogram.HorizontalLineCollection.MinValue(row);
            }
            else
            {
                c = main.Start;
                e = main.End;
                (ind, min) = data.Nonogram.HorizontalLineCollection.MinValue(row,
                    main.FirstValueIndex, main.LastValueIndex);
            }
        
            int start = -1;
            for (; c <= e; c++)
            {
                if (data.Nonogram[row, c])
                {
                    start = -2;
                    continue;
                }
                
                if (data.IsAvailable(row, c)) continue;

                if (TryProcessHorizontal(data, start, c, min, row, ind)) return;

                start = c;
            }
            
            if (TryProcessHorizontal(data, start, e + 1, min, row, ind)) return;
        }
        
        for (int col = 0; col < data.Nonogram.RowCount; col++)
        {
            int r, e, min, ind;
            var main = data.PreComputer.VerticalRemainingValuesSpace(col);
            if (main.IsInvalid())
            {
                r = 0;
                e = data.Nonogram.ColumnCount - 1;
                (ind, min) = data.Nonogram.VerticalLineCollection.MinValue(col);
            }
            else
            {
                r = main.Start;
                e = main.End;
                (ind, min) = data.Nonogram.VerticalLineCollection.MinValue(col,
                    main.FirstValueIndex, main.LastValueIndex);
            }
            
            int start = -1;
            for (; r <= e; r++)
            {
                if (data.Nonogram[r, col])
                {
                    start = -2;
                    continue;
                }
                
                if (data.IsAvailable(r, col)) continue;

                if (TryProcessVertical(data, start, r, min, col, ind)) return;

                start = r;
            }
            
            if (TryProcessVertical(data, start, e + 1, min, col, ind)) return;
        }
    }

    private bool TryProcessHorizontal(INonogramSolverData data, int start, int current, int min, int row, int ind)
    {
        if (start == -2 || start == current - 1 || current - start - 1 >= min) return false;
        
        for (int i = start + 1; i < current; i++)
        {
            data.ChangeBuffer.ProposePossibilityRemoval(row, i);
        }

        return data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new NotEnoughSpaceReportBuilder(
                row, Orientation.Horizontal, ind, start, current)) && StopOnFirstPush;
    }
    
    private bool TryProcessVertical(INonogramSolverData data, int start, int current, int min, int col, int ind)
    {
        if (start == -2 || start == current - 1 || current - start - 1 >= min) return false;
        
        for (int i = start + 1; i < current; i++)
        {
            data.ChangeBuffer.ProposePossibilityRemoval(i, col);
        }

        return data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new NotEnoughSpaceReportBuilder(
            col, Orientation.Vertical, ind, start, current)) && StopOnFirstPush;
    }
}

public class NotEnoughSpaceReportBuilder : IChangeReportBuilder<DichotomousChange, INonogramSolvingState, INonogramHighlighter>
{
    private readonly int _unit;
    private readonly Orientation _orientation;
    private readonly int _ind;
    private readonly int _start;
    private readonly int _end;

    public NotEnoughSpaceReportBuilder(int unit, Orientation orientation, int ind, int start, int end)
    {
        _unit = unit;
        _orientation = orientation;
        _ind = ind;
        _start = start;
        _end = end;
    }

    public ChangeReport<INonogramHighlighter> BuildReport(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return new ChangeReport<INonogramHighlighter>("Not Enough Space", lighter =>
        {
            lighter.HighlightValues(_orientation, _unit, _ind, _ind, StepColor.Cause1);
            lighter.EncircleLineSection(_orientation, _unit, _start + 1, _end - 1, StepColor.Cause1);
        });
    }

    public Clue<INonogramHighlighter> BuildClue(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return Clue<INonogramHighlighter>.Default();
    }
}