using Model.Core;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver;
using Model.Utility.BitSets;

namespace Model.Kakuros.Strategies;

public class AmountCoherencyStrategy : KakuroStrategy
{
    public AmountCoherencyStrategy() : base("Amount Coherency", StepDifficulty.Easy, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(IKakuroStrategyUser strategyUser)
    {
        foreach (var sum in strategyUser.Kakuro.Sums)
        {
            var total = 0;
            int withSolutions = 1;
            foreach (var cell in sum)
            {
                var n = strategyUser[cell.Row, cell.Column];
                if (n != 0)
                {
                    total += n;
                    withSolutions++;
                }
            }

            if (withSolutions > sum.Length) continue;

            var min = KakuroCellUtility.MinAmountFor(sum.Length - withSolutions);
            var max = KakuroCellUtility.MaxAmountFor(sum.Length - withSolutions);
            foreach (var cell in sum)
            {
                if (strategyUser[cell.Row, cell.Column] != 0) continue;

                var possibilities = strategyUser.PossibilitiesAt(cell);
                foreach (var p in possibilities.EnumeratePossibilities())
                {
                    if (total + p + min > sum.Amount || total + p + max < sum.Amount)
                    {
                        strategyUser.ChangeBuffer.ProposePossibilityRemoval(p, cell);
                        strategyUser.ChangeBuffer.Commit(
                            DefaultChangeReportBuilder<IUpdatableSolvingState, ISolvingStateHighlighter>.Instance);
                    }
                }
            }
        }
    }
}