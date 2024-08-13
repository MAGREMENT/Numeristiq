using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;
using Model.Utility.Collections;

namespace Model.Binairos.Strategies;

public class AdvancedUniquenessEnforcementStrategy : Strategy<IBinairoSolverData>
{
    public AdvancedUniquenessEnforcementStrategy() : base("Advanced Uniqueness Enforcement", Difficulty.Hard, InstanceHandling.FirstOnly)
    {
    }

    public override void Apply(IBinairoSolverData data)
    {
        for (int row = 0; row < data.RowCount; row++)
        {
            var set = data.Binairo.RowSetAt(row);
            if(set.GetTotalCount() != data.ColumnCount - 3) continue;

            int s, d;
            var target = data.ColumnCount / 2 - 1;
            if (set.OnesCount == target)
            {
                s = 1;
                d = 2;
            }
            else if (set.TwosCount == target)
            {
                s = 2;
                d = 1;
            }
            else continue;

            for (int col = 0; col < data.ColumnCount; col++)
            {
                if(set[col] != 0) continue;

                var buffer = set.Add(col, s);
                for (int c = 0; c < data.ColumnCount; c++)
                {
                    if (buffer[c] == 0) buffer = buffer.Add(c, d);
                }

                int r = 0;
                for (; r < data.RowCount; r++)
                {
                    if (r != row && data.Binairo.RowSetAt(r) == buffer) break;
                }

                if (r < data.RowCount)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(d, row, col);
                    if (data.ChangeBuffer.NeedCommit())
                    {
                        data.ChangeBuffer.Commit(new AdvancedUniquenessEnforcementReportBuilder(row, r, true));
                        if (StopOnFirstCommit) return;
                    }
                }
            }
        }
        
        for (int col = 0; col < data.ColumnCount; col++)
        {
            var set = data.Binairo.ColumnSetAt(col);
            if(set.GetTotalCount() != data.RowCount - 3) continue;

            int s, d;
            var target = data.RowCount / 2 - 1;
            if (set.OnesCount == target)
            {
                s = 1;
                d = 2;
            }
            else if (set.TwosCount == target)
            {
                s = 2;
                d = 1;
            }
            else continue;

            for (int row = 0; row < data.RowCount; row++)
            {
                if(set[row] != 0) continue;

                var buffer = set.Add(row, s);
                for (int r = 0; r < data.RowCount; r++)
                {
                    if (buffer[r] == 0) buffer = buffer.Add(r, d);
                }

                int c = 0;
                for (; c < data.ColumnCount; c++)
                {
                    if (c != col && data.Binairo.ColumnSetAt(c) == buffer) break;
                }

                if (c < data.ColumnCount)
                {
                    data.ChangeBuffer.ProposeSolutionAddition(d, row, col);
                    if (data.ChangeBuffer.NeedCommit())
                    {
                        data.ChangeBuffer.Commit(new AdvancedUniquenessEnforcementReportBuilder(col, c, false));
                        if (StopOnFirstCommit) return;
                    }
                }
            }
        }
    }
}

public class AdvancedUniquenessEnforcementReportBuilder : IChangeReportBuilder<BinaryChange, IBinarySolvingState,
    IBinairoHighlighter>
{
    private readonly int _unit;
    private readonly int _otherUnit;
    private readonly bool _isRow;

    public AdvancedUniquenessEnforcementReportBuilder(int unit, int otherUnit, bool isRow)
    {
        _unit = unit;
        _otherUnit = otherUnit;
        _isRow = isRow;
    }

    public ChangeReport<IBinairoHighlighter> BuildReport(IReadOnlyList<BinaryChange> changes, IBinarySolvingState snapshot)
    {
        List<Cell> otherCells = new();
        ContainingList<Cell> emptyCells = new();

        if (_isRow)
        {
            for (int col = 0; col < snapshot.ColumnCount; col++)
            {
                var n = snapshot[_unit, col];
                if (n == 0) emptyCells.Add(new Cell(_unit, col));
                
                otherCells.Add(new Cell(_otherUnit, col));
            }
        }
        else
        {
            for (int row = 0; row < snapshot.RowCount; row++)
            {
                var n = snapshot[row, _unit];
                if (n == 0) emptyCells.Add(new Cell(row, _unit));
               
                otherCells.Add(new Cell(row, _otherUnit));
            }
        }
        
        var u = _isRow ? 'r' : 'c';
        return new ChangeReport<IBinairoHighlighter>($"Advanced Uniqueness Enforcement in {u}{_unit + 1} because of {u}{_otherUnit + 1}",
            lighter =>
            {
                foreach (var cell in otherCells) lighter.HighlightCell(cell, StepColor.Cause1);
                lighter.EncircleCells(emptyCells, StepColor.Cause2);

                ChangeReportHelper.HighlightChanges(lighter, changes);
            });
    }

    public Clue<IBinairoHighlighter> BuildClue(IReadOnlyList<BinaryChange> changes, IBinarySolvingState snapshot)
    {
        return Clue<IBinairoHighlighter>.Default();
    }
}