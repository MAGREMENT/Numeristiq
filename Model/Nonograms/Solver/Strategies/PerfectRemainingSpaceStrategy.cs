using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;

namespace Model.Nonograms.Solver.Strategies;

public class PerfectRemainingSpaceStrategy : Strategy<INonogramSolverData>
{
    public PerfectRemainingSpaceStrategy() : base("Perfect Remaining Space", StepDifficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        for (int row = 0; row < data.Nonogram.RowCount; row++)
        {
            var space = data.PreComputer.HorizontalRemainingValuesSpace(row);
            if (space.IsInvalid() || data.Nonogram.HorizontalLines.NeededSpace(row,
                    space.FirstValueIndex, space.LastValueIndex) != space.End - space.Start + 1) continue;
            
            var cursor = space.Start;
            for(int index = space.FirstValueIndex; index <= space.LastValueIndex; index++)
            {
                var limit = cursor + data.Nonogram.HorizontalLines.TryGetValue(row, index);
                for (; cursor < limit; cursor++)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(row, cursor);
                }

                cursor++;
            }

            if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new PerfectRemainingSpaceStrategyReportBuilder(
                    space, row, Orientation.Horizontal)) && StopOnFirstPush) return;
        }
        
        for (int col = 0; col < data.Nonogram.ColumnCount; col++)
        {
            var space = data.PreComputer.VerticalRemainingValuesSpace(col);
            if (space.IsInvalid() ||data.Nonogram.VerticalLines.NeededSpace(col,
                    space.FirstValueIndex, space.LastValueIndex) != space.End - space.Start + 1) continue;
            
            var cursor = space.Start;
            for(int index = space.FirstValueIndex; index <= space.LastValueIndex; index++)
            {
                var limit = cursor + data.Nonogram.VerticalLines.TryGetValue(col, index);
                for (; cursor < limit; cursor++)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(cursor, col);
                }

                cursor++;
            }
            
            if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new PerfectRemainingSpaceStrategyReportBuilder(
                    space, col, Orientation.Vertical)) && StopOnFirstPush) return;
        }
    }
}

public class PerfectRemainingSpaceStrategyReportBuilder : IChangeReportBuilder<DichotomousChange, INonogramSolvingState,
        INonogramHighlighter>
{
    private readonly MultiValueSpace _space;
    private readonly int _unit;
    private readonly Orientation _orientation;

    public PerfectRemainingSpaceStrategyReportBuilder(MultiValueSpace space, int unit, Orientation orientation)
    {
        _space = space;
        _unit = unit;
        _orientation = orientation;
    }

    public ChangeReport<INonogramHighlighter> BuildReport(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return new ChangeReport<INonogramHighlighter>("Perfect Remaining Space", lighter =>
        {
            lighter.EncircleLineSection(_orientation, _unit, _space.Start, _space.End, StepColor.On);
            lighter.HighlightValues(_orientation, _unit, _space.FirstValueIndex, _space.LastValueIndex, StepColor.On);
        });
    }

    public Clue<INonogramHighlighter> BuildClue(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return Clue<INonogramHighlighter>.Default();
    }
}

