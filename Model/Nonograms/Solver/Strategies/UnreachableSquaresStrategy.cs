using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Nonograms.Solver.Strategies;

public class UnreachableSquaresStrategy : Strategy<INonogramSolverData>
{
    public UnreachableSquaresStrategy() : base("Unreachable Squares", Difficulty.Medium, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        InfiniteBitSet bitSet = new();
        List<int> pos = new();
        for (int row = 0; row < data.Nonogram.RowCount; row++)
        {
            var remaining = data.PreComputer.HorizontalRemainingValuesSpace(row);
            if (!remaining.IsInvalid() && remaining.FirstValueIndex == remaining.LastValueIndex)
            {
                var v = data.Nonogram.HorizontalLines.TryGetValue(row, remaining.FirstValueIndex);
                for (int col = remaining.Start; col <= remaining.End; col++)
                {
                    if(!data.Nonogram[row, col]) continue;

                    pos.Add(col);
                    for (int c = remaining.Start; c <= col - v; c++)
                    {
                        data.ChangeBuffer.ProposePossibilityRemoval(row, c);
                    }

                    for (int c = remaining.End; c >= col + v; c--)
                    {
                        data.ChangeBuffer.ProposePossibilityRemoval(row, c);
                    }
                }

                if (data.ChangeBuffer.NeedCommit())
                {
                    data.ChangeBuffer.Commit(new SingleValueUnreachableSquareReportBuilder(Orientation.Horizontal, row,
                        pos.ToArray(), remaining.FirstValueIndex));
                    if (StopOnFirstCommit) return;
                }
            }
            
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

            if (data.ChangeBuffer.NeedCommit())
            {
                data.ChangeBuffer.Commit(new UnreachableSquareReportBuilder(
                    spaces, Orientation.Horizontal, row));
                if (StopOnFirstCommit) return;
            }
            bitSet.Clear();
        }
        
        for (int col = 0; col < data.Nonogram.ColumnCount; col++)
        {
            var remaining = data.PreComputer.VerticalRemainingValuesSpace(col);
            if (!remaining.IsInvalid() && remaining.FirstValueIndex == remaining.LastValueIndex)
            {
                var v = data.Nonogram.VerticalLines.TryGetValue(col, remaining.FirstValueIndex);
                for (int row = remaining.Start; row <= remaining.End; row++)
                {
                    if(!data.Nonogram[row, col]) continue;

                    pos.Add(row);
                    for (int r = remaining.Start; r <= row - v; r++)
                    {
                        data.ChangeBuffer.ProposePossibilityRemoval(r, col);
                    }

                    for (int r = remaining.End; r >= row + v; r--)
                    {
                        data.ChangeBuffer.ProposePossibilityRemoval(r, col);
                    }
                }

                if (data.ChangeBuffer.NeedCommit())
                {
                    data.ChangeBuffer.Commit(new SingleValueUnreachableSquareReportBuilder(Orientation.Vertical, col,
                        pos.ToArray(), remaining.FirstValueIndex));
                    if (StopOnFirstCommit) return;
                }
            }
            
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

            if (data.ChangeBuffer.NeedCommit())
            {
                data.ChangeBuffer.Commit(new UnreachableSquareReportBuilder(spaces, Orientation.Vertical, col));
                if (StopOnFirstCommit) return;
            }
            bitSet.Clear();
        }
    }
}

public class SingleValueUnreachableSquareReportBuilder : IChangeReportBuilder<DichotomousChange, INonogramSolvingState,
    INonogramHighlighter>
{
    private readonly int[] _pos;
    private readonly Orientation _orientation;
    private readonly int _unit;
    private readonly int _valueIndex;

    public SingleValueUnreachableSquareReportBuilder(Orientation orientation, int unit, int[] pos, int valueIndex)
    {
        _pos = pos;
        _orientation = orientation;
        _unit = unit;
        _valueIndex = valueIndex;
    }

    public ChangeReport<INonogramHighlighter> BuildReport(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return new ChangeReport<INonogramHighlighter>("Unreachable Squares", lighter =>
        {
            lighter.HighlightValues(_orientation, _unit, _valueIndex, _valueIndex, StepColor.Cause1);
            foreach(var other in _pos)
            {
                lighter.EncircleLineSection(_orientation, _unit, other, other, StepColor.Cause1);
            }
        });
    }

    public Clue<INonogramHighlighter> BuildClue(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return Clue<INonogramHighlighter>.Default();
    }
}

public class UnreachableSquareReportBuilder : IChangeReportBuilder<DichotomousChange, INonogramSolvingState,
        INonogramHighlighter>
{
    private readonly IReadOnlyValueSpaceCollection _spaces;
    private readonly Orientation _orientation;
    private readonly int _unit;

    public UnreachableSquareReportBuilder(IReadOnlyValueSpaceCollection spaces, Orientation orientation, int unit)
    {
        _spaces = spaces;
        _orientation = orientation;
        _unit = unit;
    }

    public ChangeReport<INonogramHighlighter> BuildReport(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        var highlights = new Highlight<INonogramHighlighter>[_spaces.Count];
        for (int i = 0; i < _spaces.Count; i++)
        {
            var n = i;
            highlights[i] = lighter =>
            {
                lighter.HighlightValues(_orientation, _unit, _spaces.FirstValueIndex + n,
                    _spaces.FirstValueIndex + n, StepColor.Cause1);
                lighter.EncircleLineSection(_orientation, _unit, _spaces[n].Start, _spaces[n].End, StepColor.Cause1);
            };
        }

        return new ChangeReport<INonogramHighlighter>("Unreachable Squares", highlights);
    }

    public Clue<INonogramHighlighter> BuildClue(IReadOnlyList<DichotomousChange> changes, INonogramSolvingState snapshot)
    {
        return Clue<INonogramHighlighter>.Default();
    }
}