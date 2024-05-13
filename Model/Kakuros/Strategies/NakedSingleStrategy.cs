using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver;
using Model.Utility.BitSets;

namespace Model.Kakuros.Strategies;

public class NakedSingleStrategy : KakuroStrategy
{
    public NakedSingleStrategy() : base("Naked Single", StepDifficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(IKakuroStrategyUser strategyUser)
    {
        foreach (var cell in strategyUser.Kakuro.EnumerateCells())
        {
            var pos = strategyUser.PossibilitiesAt(cell);
            if (pos.Count != 1) continue;
            
            strategyUser.ChangeBuffer.ProposeSolutionAddition(pos.FirstPossibility(), cell);
            strategyUser.ChangeBuffer.Commit(
                DefaultChangeReportBuilder<IUpdatableSolvingState, ISolvingStateHighlighter>.Instance);
        }
    }
}