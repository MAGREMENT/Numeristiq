using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;
using Model.Utility.Collections;

namespace Model.Binairos.Strategies;

public class AdvancedTripleDenialStrategy : Strategy<IBinairoSolverData>
{
    public AdvancedTripleDenialStrategy() : base("Advanced Triple Denial", Difficulty.Medium, InstanceHandling.FirstOnly)
    {
    }

    public override void Apply(IBinairoSolverData data)
    {
        for (int row = 0; row < data.RowCount; row++)
        {
            var set = data.Binairo.RowSetAt(row);
            if(set.GetTotalCount() != data.ColumnCount - 3) continue;

            int d;
            var target = data.ColumnCount / 2 - 1;
            if (set.OnesCount == target) d = 2;
            else if (set.TwosCount == target) d = 1;
            else continue;

            for (int col = 0; col < data.ColumnCount; col++)
            {
                if(data[row, col] != 0) continue;

                for (int c = 0; c < data.ColumnCount - 2; c++)
                {
                    var n = data[row, c];
                    if(n != d && !(n == 0 && c != col)) continue;

                    n = data[row, c + 1];
                    if(n != d && !(n == 0 && c + 1 != col)) continue;
                    
                    n = data[row, c + 2];
                    if(n != d && !(n == 0 && c + 2 != col)) continue;

                    data.ChangeBuffer.ProposeSolutionAddition(d, row, col);
                }

                if (data.ChangeBuffer.NeedCommit())
                {
                    data.ChangeBuffer.Commit(new AdvancedTripleDenialReportBuilder(
                        row, true, BinairoUtility.Opposite(d)));
                    if (StopOnFirstCommit) return;
                }
            }
        }
        
        for (int col = 0; col < data.ColumnCount; col++)
        {
            var set = data.Binairo.ColumnSetAt(col);
            if(set.GetTotalCount() != data.RowCount - 3) continue;

            int d;
            var target = data.RowCount / 2 - 1;
            if (set.OnesCount == target) d = 2;
            else if (set.TwosCount == target) d = 1;
            else continue;

            for (int row = 0; row < data.RowCount; row++)
            {
                if(data[row, col] != 0) continue;

                for (int r = 0; r < data.RowCount - 2; r++)
                {
                    var n = data[r, col];
                    if(n != d && !(n == 0 && r != row)) continue;

                    n = data[r + 1, col];
                    if(n != d && !(n == 0 && r + 1 != row)) continue;
                    
                    n = data[r + 2, col];
                    if(n != d && !(n == 0 && r + 2 != row)) continue;

                    data.ChangeBuffer.ProposeSolutionAddition(d, row, col);
                }

                if (data.ChangeBuffer.NeedCommit())
                {
                    data.ChangeBuffer.Commit(new AdvancedTripleDenialReportBuilder(col, false,
                        BinairoUtility.Opposite(d)));
                    if (StopOnFirstCommit) return;
                }
            }
        }
    }
}

public class AdvancedTripleDenialReportBuilder : IChangeReportBuilder<BinaryChange, IBinarySolvingState, IBinairoHighlighter>
{
    private readonly int _unit;
    private readonly bool _isRow;
    private readonly int _cause;

    public AdvancedTripleDenialReportBuilder(int unit, bool isRow, int cause)
    {
        _unit = unit;
        _isRow = isRow;
        _cause = cause;
    }

    public ChangeReport<IBinairoHighlighter> BuildReport(IReadOnlyList<BinaryChange> changes, IBinarySolvingState snapshot)
    {
        List<Cell> causeCells = new();
        ContainingList<Cell> emptyCells = new();

        if (_isRow)
        {
            for (int col = 0; col < snapshot.ColumnCount; col++)
            {
                var n = snapshot[_unit, col];
                if (n == 0) emptyCells.Add(new Cell(_unit, col));
                else if(n == _cause) causeCells.Add(new Cell(_unit, col));
            }
        }
        else
        {
            for (int row = 0; row < snapshot.RowCount; row++)
            {
                var n = snapshot[row, _unit];
                if (n == 0) emptyCells.Add(new Cell(row, _unit));
                else if(n == _cause) causeCells.Add(new Cell(row, _unit));
            }
        }
        
        return new ChangeReport<IBinairoHighlighter>($"Advanced Triple Denial in {emptyCells.ToStringSequence(", ")}" +
                                                     $"for {BinairoUtility.Opposite(_cause)}", lighter =>
        {
            foreach (var cell in causeCells) lighter.HighlightCell(cell, StepColor.Cause1);
            lighter.EncircleCells(emptyCells, StepColor.Cause2);
            
            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    public Clue<IBinairoHighlighter> BuildClue(IReadOnlyList<BinaryChange> changes, IBinarySolvingState snapshot)
    {
        return Clue<IBinairoHighlighter>.Default();
    }
}