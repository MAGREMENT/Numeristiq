using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace Model.Binairos.Strategies;

public class TripleDenialStrategy : Strategy<IBinairoSolverData>
{
    public TripleDenialStrategy() : base("Triple Denial", Difficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(IBinairoSolverData data)
    {
        for (int row = 0; row < data.RowCount; row++)
        {
            for (int col = 0; col < data.ColumnCount - 2; col++)
            {
                var n = data[row, col];
                if(n == 0 || data[row, col + 1] != 0 || data[row, col + 2] != n) continue;
                
                data.ChangeBuffer.ProposeSolutionAddition(BinairoUtility.Opposite(n), row, col + 1);
                if (data.ChangeBuffer.NeedCommit())
                {
                    data.ChangeBuffer.Commit(new TripleDenialReportBuilder());
                    if(StopOnFirstCommit) return;
                }
            }
        }
        
        for (int col = 0; col < data.ColumnCount; col++)
        {
            for (int row = 0; row < data.RowCount - 2; row++)
            {
                var n = data[row, col];
                if(n == 0 || data[row + 1, col] != 0 || data[row + 2, col] != n) continue;
                
                data.ChangeBuffer.ProposeSolutionAddition(BinairoUtility.Opposite(n), row + 1, col);
                if (data.ChangeBuffer.NeedCommit())
                {
                    data.ChangeBuffer.Commit(new TripleDenialReportBuilder());
                    if(StopOnFirstCommit) return;
                }
            }
        }
    }
}

public class TripleDenialReportBuilder : IChangeReportBuilder<BinaryChange, IBinarySolvingState, IBinairoHighlighter>
{
    public ChangeReport<IBinairoHighlighter> BuildReport(IReadOnlyList<BinaryChange> changes, IBinarySolvingState snapshot)
    {
        return new ChangeReport<IBinairoHighlighter>("Triple Denial");
    }

    public Clue<IBinairoHighlighter> BuildClue(IReadOnlyList<BinaryChange> changes, IBinarySolvingState snapshot)
    {
        return Clue<IBinairoHighlighter>.Default();
    }
}