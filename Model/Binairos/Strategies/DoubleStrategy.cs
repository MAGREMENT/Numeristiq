using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;

namespace Model.Binairos.Strategies;

public class DoubleStrategy : Strategy<IBinairoSolverData>
{
    public DoubleStrategy() : base("Double", Difficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(IBinairoSolverData data)
    {
        for (int row = 0; row < data.RowCount; row++)
        {
            var last = 0;
            for (int col = 0; col < data.ColumnCount; col++)
            {
                var n = data.Binairo[row, col];
                if (n != 0 && n == last)
                {
                    var o = BinairoUtility.Opposite(n);
                    var c = col - 2;
                    if (c > 0) data.ChangeBuffer.ProposeSolutionAddition(o, row, c);
                    c = col + 1;
                    if (c < data.ColumnCount) data.ChangeBuffer.ProposeSolutionAddition(o, row, c);

                    if (data.ChangeBuffer.NeedCommit())
                    {
                        data.ChangeBuffer.Commit(new DoubleReportBuilder(row, col, true));
                        if(StopOnFirstCommit) return;
                    }
                }

                last = n;
            }
        }
        
        for (int col = 0; col < data.ColumnCount; col++)
        {
            var last = 0;
            for (int row = 0; row < data.RowCount; row++)
            {
                var n = data.Binairo[row, col];
                if (n != 0 && n == last)
                {
                    var o = BinairoUtility.Opposite(n);
                    var r = row - 2;
                    if (r > 0) data.ChangeBuffer.ProposeSolutionAddition(o, r, col);
                    r = row + 1;
                    if (r < data.RowCount) data.ChangeBuffer.ProposeSolutionAddition(o, r, col);

                    if (data.ChangeBuffer.NeedCommit())
                    {
                        data.ChangeBuffer.Commit(new DoubleReportBuilder(row, col, false));
                        if(StopOnFirstCommit) return;
                    }
                }

                last = n;
            }
        }
    }
}

public class DoubleReportBuilder : IChangeReportBuilder<BinaryChange, IBinarySolvingState, IBinairoHighlighter>
{
    private readonly int _row;
    private readonly int _col;
    private readonly bool _isRow;

    public DoubleReportBuilder(int row, int col, bool isRow)
    {
        _row = row;
        _col = col;
        _isRow = isRow;
    }

    public ChangeReport<IBinairoHighlighter> BuildReport(IReadOnlyList<BinaryChange> changes, IBinarySolvingState snapshot)
    {
        var c1 = _isRow ? new Cell(_row, _col - 1) : new Cell(_row - 1, _col);
        var c2 = new Cell(_row, _col);
        
        return new ChangeReport<IBinairoHighlighter>($"Double in {c1} and {c2}", lighter =>
        {
            lighter.HighlightCell(c1, StepColor.Cause1);
            lighter.HighlightCell(c2, StepColor.Cause1);
        });
    }

    public Clue<IBinairoHighlighter> BuildClue(IReadOnlyList<BinaryChange> changes, IBinarySolvingState snapshot)
    {
        return Clue<IBinairoHighlighter>.Default();
    }
}