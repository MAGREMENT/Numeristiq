using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver;
using Model.Utility.BitSets;

namespace Model.Kakuros.Strategies;

public class NakedSingleStrategy : Strategy<IKakuroSolverData>
{
    public NakedSingleStrategy() : base("Naked Single", StepDifficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(IKakuroSolverData solverData)
    {
        foreach (var cell in solverData.Kakuro.EnumerateCells())
        {
            var pos = solverData.PossibilitiesAt(cell);
            if (pos.Count != 1) continue;
            
            solverData.ChangeBuffer.ProposeSolutionAddition(pos.FirstPossibility(), cell);
            solverData.ChangeBuffer.Commit(
                DefaultNumericChangeReportBuilder<IUpdatableNumericSolvingState, INumericSolvingStateHighlighter>.Instance);
        }
    }
}