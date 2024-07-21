using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;

namespace Model.Nonograms.Solver.Strategies;

public class EdgeValueStrategy : Strategy<INonogramSolverData>
{
    public EdgeValueStrategy() : base("Edge Value", StepDifficulty.Easy, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        for (int row = 0; row < data.Nonogram.RowCount; row++)
        {
            var spaces = data.PreComputer.HorizontalValueSpaces(row);
            if (spaces.Count == 0) continue;
            
            var space = spaces[0];
            if (space.IsValid() && data.Nonogram[row, space.Start])
            {
                for (int current = space.Start + 1; current < space.Start + space.Value; current++)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(row, current);
                }

                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new EdgeValueReportBuilder(
                        Orientation.Horizontal, row, space, spaces.FirstValueIndex)) && StopOnFirstPush) return;
            }

            space = spaces[^1];
            if (space.IsValid() && data.Nonogram[row, space.End])
            {
                for (int current = space.End - 1; current > space.End - space.Value; current--)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(row, current);
                }
                
                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new EdgeValueReportBuilder(
                        Orientation.Horizontal, row, space, spaces.LastValueIndex)) && StopOnFirstPush) return;
            }
        }
        
        for (int col = 0; col < data.Nonogram.ColumnCount; col++)
        {
            var spaces = data.PreComputer.VerticalValueSpaces(col);
            if (spaces.Count == 0) continue;
            
            var space = spaces[0];
            if (space.IsValid() && data.Nonogram[space.Start, col])
            {
                for (int current = space.Start + 1; current < space.Start + space.Value; current++)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(current, col);
                }

                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new EdgeValueReportBuilder(
                        Orientation.Vertical, col, space, spaces.FirstValueIndex)) && StopOnFirstPush) return;
            }

            space = spaces[^1];
            if (space.IsValid() && data.Nonogram[space.End, col])
            {
                for (int current = space.End - 1; current > space.End - space.Value; current--)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(current, col);
                }
                
                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new EdgeValueReportBuilder(
                        Orientation.Vertical, col, space, spaces.LastValueIndex)) && StopOnFirstPush) return;
            }
        }
    }
}

public class EdgeValueReportBuilder : IChangeReportBuilder<DichotomousChange, INonogramSolvingState, INonogramHighlighter>
{
    private readonly Orientation _orientation;
    private readonly int _unit;
    private readonly ValueSpace _space;
    private readonly int _valueIndex;

    public EdgeValueReportBuilder(Orientation orientation, int unit, ValueSpace space, int valueIndex)
    {
        _orientation = orientation;
        _unit = unit;
        _space = space;
        _valueIndex = valueIndex;
    }

    public ChangeReport<INonogramHighlighter> BuildReport(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return new ChangeReport<INonogramHighlighter>("Edge Value", lighter =>
        {
            lighter.EncircleLineSection(_orientation, _unit, _space.Start, _space.End, StepColor.On);
            lighter.HighlightValues(_orientation, _unit, _valueIndex, _valueIndex, StepColor.On);
        });
    }

    public Clue<INonogramHighlighter> BuildClue(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return Clue<INonogramHighlighter>.Default();
    }
}