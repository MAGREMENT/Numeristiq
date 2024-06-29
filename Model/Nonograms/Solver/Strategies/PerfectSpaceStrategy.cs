using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;

namespace Model.Nonograms.Solver.Strategies;

public class PerfectSpaceStrategy : Strategy<INonogramSolverData>
{
    public PerfectSpaceStrategy() : base("Perfect Space", StepDifficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        for (int row = 0; row < data.Nonogram.RowCount; row++)
        {
            var space = data.PreComputer.HorizontalMainSpace(row);
            if (space.IsInvalid() || data.Nonogram.HorizontalLineCollection.NeededSpace(row,
                    space.FirstValueIndex, space.LastValueIndex) != space.End - space.Start + 1) continue;

            var cursor = space.Start;
            for(int index = space.FirstValueIndex; index <= space.LastValueIndex; index++)
            {
                var limit = cursor + data.Nonogram.HorizontalLineCollection.TryGetValue(row, index);
                for (; cursor < limit; cursor++)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(row, cursor);
                }

                cursor++;
            }

            if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(DefaultDichotomousChangeReportBuilder<
                    IDichotomousSolvingState, INonogramHighlighter>.Instance) && StopOnFirstPush) return;
        }
        
        for (int col = 0; col < data.Nonogram.ColumnCount; col++)
        {
            var space = data.PreComputer.VerticalMainSpace(col);
            if (space.IsInvalid() || data.Nonogram.VerticalLineCollection.NeededSpace(col,
                    space.FirstValueIndex, space.LastValueIndex) != space.End - space.Start + 1) continue;

            var cursor = space.Start;
            for(int index = space.FirstValueIndex; index <= space.LastValueIndex; index++)
            {
                var limit = cursor + data.Nonogram.VerticalLineCollection.TryGetValue(col, index);
                for (; cursor < limit; cursor++)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(cursor, col);
                }

                cursor++;
            }
            
            if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(DefaultDichotomousChangeReportBuilder<
                    IDichotomousSolvingState, INonogramHighlighter>.Instance) && StopOnFirstPush) return;
        }
    }
}

public class PerfectSpaceStrategyReportBuilder : IChangeReportBuilder<DichotomousChange, INonogramSolvingState,
        INonogramHighlighter>
{
    private readonly MainSpace _space;
    private readonly int _unit;
    private readonly Orientation _orientation;

    public PerfectSpaceStrategyReportBuilder(MainSpace space, int unit, Orientation orientation)
    {
        _space = space;
        _unit = unit;
        _orientation = orientation;
    }

    public ChangeReport<INonogramHighlighter> BuildReport(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return new ChangeReport<INonogramHighlighter>("Perfect Space", lighter =>
        {
            lighter.EncircleCells(_space.EnumerateCells(_orientation, _unit), ChangeColoration.CauseOnOne);
            lighter.EncircleValues(_orientation, _unit, _space.FirstValueIndex, _space.LastValueIndex, ChangeColoration.CauseOnOne);
        });
    }

    public Clue<INonogramHighlighter> BuildClue(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        throw new System.NotImplementedException();
    }
}

