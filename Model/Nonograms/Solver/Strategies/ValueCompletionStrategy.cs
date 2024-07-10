using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;

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
            var spaces = data.PreComputer.HorizontalValueSpaces(row);
            for(int i = 0; i < spaces.Count; i++)
            {
                var space = spaces[i];
                if (!space.IsCompleted(data.Nonogram, row, Orientation.Horizontal)) continue;

                if (space.Start > 0) data.ChangeBuffer.ProposePossibilityRemoval(row, space.Start - 1);
                if (space.End < data.Nonogram.ColumnCount - 1)
                    data.ChangeBuffer.ProposePossibilityRemoval(row, space.End + 1);

                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new ValueCompletionReportBuilder(
                        Orientation.Horizontal, row, space, i)) && StopOnFirstPush) return;
            }
        }
        
        for (int col = 0; col < data.Nonogram.ColumnCount; col++)
        {
            var spaces = data.PreComputer.VerticalValueSpaces(col);
            for(int i = 0; i < spaces.Count; i++)
            {
                var space = spaces[i];
                if (!space.IsCompleted(data.Nonogram, col, Orientation.Vertical)) continue;

                if (space.Start > 0) data.ChangeBuffer.ProposePossibilityRemoval(space.Start - 1, col);
                if (space.End < data.Nonogram.RowCount - 1)
                    data.ChangeBuffer.ProposePossibilityRemoval(space.End + 1, col);

                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new ValueCompletionReportBuilder(
                    Orientation.Vertical, col, space, i)) && StopOnFirstPush) return;
            }
        }
    }
}

public class ValueCompletionReportBuilder : IChangeReportBuilder<DichotomousChange, INonogramSolvingState, INonogramHighlighter>
{
    private readonly Orientation _orientation;
    private readonly int _unit;
    private readonly ValueSpace _space;
    private readonly int _valueIndex;

    public ValueCompletionReportBuilder(Orientation orientation, int unit, ValueSpace space, int valueIndex)
    {
        _orientation = orientation;
        _unit = unit;
        _space = space;
        _valueIndex = valueIndex;
    }

    public ChangeReport<INonogramHighlighter> BuildReport(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return new ChangeReport<INonogramHighlighter>("Value Completion", lighter =>
        {
            lighter.EncircleLineSection(_orientation, _unit, _space.Start, _space.End, ChangeColoration.CauseOnOne);
            lighter.HighlightValues(_orientation, _unit, _valueIndex, _valueIndex, ChangeColoration.CauseOnOne);
        });
    }

    public Clue<INonogramHighlighter> BuildClue(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return Clue<INonogramHighlighter>.Default();
    }
}