using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver;
using Model.Utility.BitSets;

namespace Model.Kakuros.Strategies;

public class NakedSingleStrategy : Strategy<IKakuroSolverData>
{
    public NakedSingleStrategy() : base("Naked Single", Difficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(IKakuroSolverData data)
    {
        foreach (var cell in data.Kakuro.EnumerateCells())
        {
            var pos = data.PossibilitiesAt(cell);
            if (pos.Count != 1) continue;
            
            data.ChangeBuffer.ProposeSolutionAddition(pos.FirstPossibility(), cell);
            data.ChangeBuffer.Commit(
                DefaultNumericChangeReportBuilder<INumericSolvingState, INumericSolvingStateHighlighter>.Instance);
        }
    }
}