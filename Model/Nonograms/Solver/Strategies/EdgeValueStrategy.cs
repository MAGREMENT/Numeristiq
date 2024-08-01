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
            var remaining = data.PreComputer.HorizontalRemainingValuesSpace(row);
            if (!remaining.IsInvalid() && remaining.FirstValueIndex == remaining.LastValueIndex)
            {
                for (int col = remaining.Start; col <= remaining.End; col++)
                {
                    if(!data.Nonogram[row, col]) continue;

                    var v = data.Nonogram.HorizontalLines.TryGetValue(row, remaining.FirstValueIndex);
                    if (col == 0 || (!data.Nonogram[row, col - 1] && !data.IsAvailable(row, col - 1)))
                    {
                        for (int current = col + 1; current < col + v; current++)
                        {
                            data.ChangeBuffer.ProposeSolutionAddition(row, current);
                        }

                        if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new EdgeValueReportBuilder(
                                Orientation.Horizontal, row, col, remaining.FirstValueIndex)) && StopOnFirstPush) return;
                    }

                    if (col == data.Nonogram.ColumnCount - 1 || (!data.Nonogram[row, col + 1] 
                                                                 && !data.IsAvailable(row, col + 1)))
                    {
                        for (int current = col - 1; current > col - v; current--)
                        {
                            data.ChangeBuffer.ProposeSolutionAddition(row, current);
                        }
                
                        if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new EdgeValueReportBuilder(
                                Orientation.Horizontal, row, col, remaining.LastValueIndex)) && StopOnFirstPush) return;
                    }
                }
            }
            
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
            var remaining = data.PreComputer.VerticalRemainingValuesSpace(col);
            if (!remaining.IsInvalid() && remaining.FirstValueIndex == remaining.LastValueIndex)
            {
                for (int row = remaining.Start; row <= remaining.End; row++)
                {
                    if(!data.Nonogram[row, col]) continue;

                    var v = data.Nonogram.VerticalLines.TryGetValue(col, remaining.FirstValueIndex);
                    if (row == 0 || (!data.Nonogram[row - 1, col] && !data.IsAvailable(row - 1, col)))
                    {
                        for (int current = row + 1; current < row + v; current++)
                        {
                            data.ChangeBuffer.ProposeSolutionAddition(current, col);
                        }

                        if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new EdgeValueReportBuilder(
                                Orientation.Vertical, col, row, remaining.FirstValueIndex)) && StopOnFirstPush) return;
                    }

                    if (row == data.Nonogram.RowCount - 1 || (!data.Nonogram[row + 1, col] 
                                                              && !data.IsAvailable(row + 1, col)))
                    {
                        for (int current = row - 1; current > row - v; current--)
                        {
                            data.ChangeBuffer.ProposeSolutionAddition(current, col);
                        }
                
                        if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new EdgeValueReportBuilder(
                                Orientation.Vertical, col, row, remaining.LastValueIndex)) && StopOnFirstPush) return;
                    }
                }
            }
            
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
    private readonly int _start;
    private readonly int _end;
    private readonly int _valueIndex;

    public EdgeValueReportBuilder(Orientation orientation, int unit, ValueSpace space, int valueIndex)
    {
        _orientation = orientation;
        _unit = unit;
        _start = space.Start;
        _end = space.End;
        _valueIndex = valueIndex;
    }

    public EdgeValueReportBuilder(Orientation orientation, int unit, int pos, int valueIndex)
    {
        _orientation = orientation;
        _unit = unit;
        _start = pos;
        _end = pos;
        _valueIndex = valueIndex;
    }

    public ChangeReport<INonogramHighlighter> BuildReport(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return new ChangeReport<INonogramHighlighter>("Edge Value", lighter =>
        {
            lighter.EncircleLineSection(_orientation, _unit, _start, _end, StepColor.On);
            lighter.HighlightValues(_orientation, _unit, _valueIndex, _valueIndex, StepColor.On);
        });
    }

    public Clue<INonogramHighlighter> BuildClue(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return Clue<INonogramHighlighter>.Default();
    }
}