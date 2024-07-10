using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;

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
            var spaces = data.PreComputer.HorizontalValueSpaces(row);
            for(int i = 0; i < spaces.Count; i++)
            {
                var space = spaces[i];
                if (space.GetLength() <= space.Value) continue;
                
                for (int col = space.End - space.Value + 1; col < space.Start + space.Value; col++)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(row, col);
                }

                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new ValueOverlayReportBuilder(
                        Orientation.Horizontal, row, space, i)) && StopOnFirstPush) return;
            }
        }
        
        for (int col = 0; col < data.Nonogram.ColumnCount; col++)
        {
            var spaces = data.PreComputer.VerticalValueSpaces(col);
            for(int i = 0; i < spaces.Count; i++)
            {
                var space = spaces[i];
                if (space.GetLength() <= space.Value) continue;
                
                for (int row = space.End - space.Value + 1; row < space.Start + space.Value; row++)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(row, col);
                }

                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new ValueOverlayReportBuilder(
                        Orientation.Vertical, col, space, i)) && StopOnFirstPush) return;
            }
        }
    }
}

public class ValueOverlayReportBuilder : IChangeReportBuilder<DichotomousChange, INonogramSolvingState, INonogramHighlighter>
{
    private readonly Orientation _orientation;
    private readonly int _unit;
    private readonly ValueSpace _space;
    private readonly int _valueIndex;

    public ValueOverlayReportBuilder(Orientation orientation, int unit, ValueSpace space, int valueIndex)
    {
        _orientation = orientation;
        _unit = unit;
        _space = space;
        _valueIndex = valueIndex;
    }

    public ChangeReport<INonogramHighlighter> BuildReport(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return new ChangeReport<INonogramHighlighter>("Value Overlay", lighter =>
        {
            lighter.EncircleLineSection(_orientation, _unit, _space.Start, _space.End, ChangeColoration.CauseOnOne);
            lighter.HighlightValues(_orientation, _unit, _valueIndex, _valueIndex, ChangeColoration.CauseOnOne);
        }, lighter =>
        {
            lighter.EncircleLineSection(_orientation, _unit, _space.Start, _space.Start + _space.Value - 1, ChangeColoration.CauseOnOne);
        }, lighter =>
        {
            lighter.EncircleLineSection(_orientation, _unit, _space.End - _space.Value + 1, _space.End, ChangeColoration.CauseOnOne);
        });
    }

    public Clue<INonogramHighlighter> BuildClue(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return Clue<INonogramHighlighter>.Default();
    }
}