using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace Model.Binairos.Strategies;

public class DoubleStrategy : Strategy<IBinarySolverData>
{
    public DoubleStrategy() : base("Double", Difficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(IBinarySolverData data)
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

                    if (data.ChangeBuffer.NeedCommit() && data.ChangeBuffer.Commit(new DoubleReportBuilder())
                                                     && StopOnFirstCommit) return;
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

                    if (data.ChangeBuffer.NeedCommit() && data.ChangeBuffer.Commit(new DoubleReportBuilder())
                                                     && StopOnFirstCommit) return;
                }

                last = n;
            }
        }
    }
}

public class DoubleReportBuilder : IChangeReportBuilder<BinaryChange, IBinarySolvingState, IBinairoHighlighter>
{
    public ChangeReport<IBinairoHighlighter> BuildReport(IReadOnlyList<BinaryChange> changes, IBinarySolvingState snapshot)
    {
        return new ChangeReport<IBinairoHighlighter>("");
    }

    public Clue<IBinairoHighlighter> BuildClue(IReadOnlyList<BinaryChange> changes, IBinarySolvingState snapshot)
    {
        return Clue<IBinairoHighlighter>.Default();
    }
}