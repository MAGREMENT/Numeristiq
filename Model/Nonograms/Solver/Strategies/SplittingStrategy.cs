using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;

namespace Model.Nonograms.Solver.Strategies;

public class SplittingStrategy : Strategy<INonogramSolverData>
{
    public SplittingStrategy() : base("Splitting", Difficulty.Easy, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        for (int row = 0; row < data.Nonogram.RowCount; row++)
        {
            var space = data.PreComputer.HorizontalRemainingValuesSpace(row);
            if (space.IsInvalid()) continue;

            var (ind, max) = data.Nonogram.HorizontalLines.MaxValue(row, space.FirstValueIndex, space.LastValueIndex);
            for (int c = 0; c < data.Nonogram.ColumnCount; c++)
            {
                if (!data.IsAvailable(row, c)) continue;

                var s = c;
                while (s > 0 && data.Nonogram[row, s - 1]) s--;
                
                var e = c;
                while (e < data.Nonogram.ColumnCount - 1 && data.Nonogram[row, e + 1]) e++;

                if (s == c || e == c || e - s + 1 <= max) continue;

                data.ChangeBuffer.ProposePossibilityRemoval(row, c);
                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new SplittingReportBuilder(
                        Orientation.Horizontal, row, s, e, ind)) && StopOnFirstPush) return;
            }
        }
        
        for (int col = 0; col < data.Nonogram.ColumnCount; col++)
        {
            var space = data.PreComputer.VerticalRemainingValuesSpace(col);
            if (space.IsInvalid()) continue;

            var (ind, max) = data.Nonogram.VerticalLines.MaxValue(col, space.FirstValueIndex, space.LastValueIndex);
            for (int r = 0; r < data.Nonogram.RowCount; r++)
            {
                if (!data.IsAvailable(r, col)) continue;

                var s = r;
                while (s > 0 && data.Nonogram[s - 1, col]) s--;
                
                var e = r;
                while (e < data.Nonogram.RowCount - 1 && data.Nonogram[e + 1, col]) e++;

                if (s == r || e == r || e - s + 1 <= max) continue;

                data.ChangeBuffer.ProposePossibilityRemoval(r, col);
                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new SplittingReportBuilder(
                        Orientation.Vertical, col, s, e, ind)) && StopOnFirstPush) return;
            } 
        }
    }
}

public class SplittingReportBuilder : IChangeReportBuilder<DichotomousChange, INonogramSolvingState, INonogramHighlighter>
{
    private readonly Orientation _orientation;
    private readonly int _unit;
    private readonly int _start;
    private readonly int _end;
    private readonly int _v;

    public SplittingReportBuilder(Orientation orientation, int unit, int start, int end, int v)
    {
        _orientation = orientation;
        _unit = unit;
        _start = start;
        _end = end;
        _v = v;
    }

    public ChangeReport<INonogramHighlighter> BuildReport(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return new ChangeReport<INonogramHighlighter>("Splitting", lighter =>
        {
            lighter.HighlightValues(_orientation, _unit, _v, _v, StepColor.Cause1);
            var middle = _orientation == Orientation.Horizontal ? changes[0].Column : changes[0].Row;
            lighter.EncircleLineSection(_orientation, _unit, _start, middle - 1, StepColor.Cause1);
            lighter.EncircleLineSection(_orientation, _unit, middle + 1, _end, StepColor.Cause1);
        });
    }

    public Clue<INonogramHighlighter> BuildClue(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return Clue<INonogramHighlighter>.Default();
    }
}