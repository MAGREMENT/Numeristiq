using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Nonograms.Solver.Strategies;

public class UnreachableSquareStrategy : Strategy<INonogramSolverData>
{
    public UnreachableSquareStrategy() : base("Unreachable Square", StepDifficulty.Medium, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        InfiniteBitSet bitSet = new();
        for (int row = 0; row < data.Nonogram.RowCount; row++)
        {
            var spaces = data.PreComputer.HorizontalValueSpaces(row);
            if (spaces.Count == 0) continue;

            foreach (var space in spaces)
            {
                for (int i = space.Start; i <= space.End; i++)
                {
                    bitSet.Add(i);
                }
            }

            for (int col = 0; col < data.Nonogram.ColumnCount; col++)
            {
                if (!bitSet.Contains(col)) data.ChangeBuffer.ProposePossibilityRemoval(row, col);
            }

            if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new UnreachableSquareReportBuilder(
                    spaces, Orientation.Horizontal, row, data.PreComputer.HorizontalMainSpace(row))) && StopOnFirstPush) return;
            bitSet.Clear();
        }
        
        for (int col = 0; col < data.Nonogram.RowCount; col++)
        {
            var spaces = data.PreComputer.VerticalValueSpaces(col);
            if (spaces.Count == 0) continue;

            foreach (var space in spaces)
            {
                for (int i = space.Start; i <= space.End; i++)
                {
                    bitSet.Add(i);
                }
            }

            for (int row = 0; row < data.Nonogram.ColumnCount; row++)
            {
                if (!bitSet.Contains(row)) data.ChangeBuffer.ProposePossibilityRemoval(row, col);
            }

            if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(new UnreachableSquareReportBuilder(
                    spaces, Orientation.Vertical, col, data.PreComputer.VerticalMainSpace(col))) && StopOnFirstPush) return;
            bitSet.Clear();
        }
    }
}

public class UnreachableSquareReportBuilder : IChangeReportBuilder<DichotomousChange, INonogramSolvingState,
        INonogramHighlighter>
{
    private readonly MainSpace _space;
    private readonly IReadOnlyList<ValueSpace> _spaces;
    private readonly Orientation _orientation;
    private readonly int _unit;

    public UnreachableSquareReportBuilder(IReadOnlyList<ValueSpace> spaces, Orientation orientation, int unit, MainSpace space)
    {
        _spaces = spaces;
        _orientation = orientation;
        _unit = unit;
        _space = space;
    }

    public ChangeReport<INonogramHighlighter> BuildReport(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        var highlights = new Highlight<INonogramHighlighter>[_spaces.Count];
        for (int i = 0; i < _spaces.Count; i++)
        {
            var n = i;
            highlights[i] = lighter =>
            {
                lighter.HighlightValues(_orientation, _unit, _space.FirstValueIndex + n,
                    _space.LastValueIndex + n, ChangeColoration.CauseOffOne);
                lighter.EncircleLineSection(_orientation, _unit, _spaces[n].Start, _spaces[n].End, ChangeColoration.CauseOffOne);
            };
        }

        return new ChangeReport<INonogramHighlighter>("", highlights);
    }

    public Clue<INonogramHighlighter> BuildClue(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        throw new System.NotImplementedException();
    }
}