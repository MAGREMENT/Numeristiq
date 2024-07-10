using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;

namespace Model.Nonograms.Solver.Strategies;

public class PerfectValueSpaceStrategy : Strategy<INonogramSolverData>
{
    public PerfectValueSpaceStrategy() : base("Perfect Value Space", StepDifficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        for (int row = 0; row < data.Nonogram.RowCount; row++)
        {
            var spaces = data.PreComputer.HorizontalValueSpaces(row);
            for (int i = 0; i < spaces.Count; i++)
            {
                var space = spaces[i];
                if (space.GetLength() != space.Value) continue;

                for (int c = space.Start; c <= space.End; c++)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(row, c);
                }

                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new PerfectValueSpaceStrategyReportBuilder(
                        space, row, Orientation.Horizontal, spaces.FirstValueIndex + i))) ;
            }
        }
        
        for (int col = 0; col < data.Nonogram.ColumnCount; col++)
        {
            var spaces = data.PreComputer.VerticalValueSpaces(col);
            for (int i = 0; i < spaces.Count; i++)
            {
                var space = spaces[i];
                if (space.GetLength() != space.Value) continue;

                for (int r = space.Start; r <= space.End; r++)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(r, col);
                }

                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new PerfectValueSpaceStrategyReportBuilder(
                        space, col, Orientation.Vertical, spaces.FirstValueIndex + i))) ;
            }
        }
    }
}

public class PerfectValueSpaceStrategyReportBuilder : IChangeReportBuilder<DichotomousChange, INonogramSolvingState,
    INonogramHighlighter>
{
    private readonly ValueSpace _space;
    private readonly int _unit;
    private readonly Orientation _orientation;
    private readonly int _valueIndex;

    public PerfectValueSpaceStrategyReportBuilder(ValueSpace space, int unit, Orientation orientation, int valueIndex)
    {
        _space = space;
        _unit = unit;
        _orientation = orientation;
        _valueIndex = valueIndex;
    }

    public ChangeReport<INonogramHighlighter> BuildReport(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return new ChangeReport<INonogramHighlighter>("Perfect Remaining Space", lighter =>
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