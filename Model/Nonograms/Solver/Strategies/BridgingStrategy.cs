using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;

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

                    if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new BridgingReportBuilder(
                            Orientation.Horizontal, row, c, last, space.FirstValueIndex)) && StopOnFirstPush) return;
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

                    if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new BridgingReportBuilder(
                            Orientation.Vertical, col, r, last, space.FirstValueIndex)) && StopOnFirstPush) return;
                }

                last = r;
            }
        }
    }
}

public class BridgingReportBuilder : IChangeReportBuilder<DichotomousChange, INonogramSolvingState, INonogramHighlighter>
{
    private readonly Orientation _orientation;
    private readonly int _unit;
    private readonly int _current;
    private readonly int _last;
    private readonly int _valueIndex;

    public BridgingReportBuilder(Orientation orientation, int unit, int current, int last, int valueIndex)
    {
        _orientation = orientation;
        _unit = unit;
        _current = current;
        _last = last;
        _valueIndex = valueIndex;
    }

    public ChangeReport<INonogramHighlighter> BuildReport(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return new ChangeReport<INonogramHighlighter>("Bridging", lighter =>
        {
            lighter.HighlightValues(_orientation, _unit, _valueIndex, _valueIndex, ChangeColoration.CauseOnOne);
            lighter.EncircleLineSection(_orientation, _unit, _current, _current, ChangeColoration.CauseOnOne);
            lighter.EncircleLineSection(_orientation, _unit, _last, _last, ChangeColoration.CauseOnOne);
        });
    }

    public Clue<INonogramHighlighter> BuildClue(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return Clue<INonogramHighlighter>.Default();
    }
}