using Model.Core;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Kakuros.Strategies;

public class CombinationCoherencyStrategy : KakuroStrategy
{
    public CombinationCoherencyStrategy() : base("Combination Coherency", StepDifficulty.Medium, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(IKakuroStrategyUser strategyUser)
    {
        foreach (var sum in strategyUser.Kakuro.Sums)
        {
            var used = new ReadOnlyBitSet16();
            int total = 0;
            foreach (var cell in sum)
            {
                var n = strategyUser[cell.Row, cell.Column];
                if (n != 0)
                {
                    used += n;
                    total += n;
                }
            }

            if (used.Count == sum.Length) continue;

            foreach (var cell in sum)
            {
                var possibilities = strategyUser.PossibilitiesAt(cell);
                if (possibilities.Count == 0) continue;

                foreach (var p in possibilities.EnumeratePossibilities())
                {
                    if (IsPossible(strategyUser, used + p, cell, sum, total + p, 0)) continue;

                    strategyUser.ChangeBuffer.ProposePossibilityRemoval(p, cell);
                    strategyUser.ChangeBuffer.Commit(
                        DefaultChangeReportBuilder<IUpdatableSolvingState, ISolvingStateHighlighter>.Instance);
                }
            }
        }
    }

    private static bool IsPossible(IKakuroStrategyUser strategyUser, ReadOnlyBitSet16 used, 
        Cell cell, IKakuroSum sum, int total, int start)
    {
        for (; start < sum.Length; start++)
        {
            var c = sum[start];
            if (c == cell) continue;

            var pos = strategyUser.PossibilitiesAt(c);
            if (pos.Count == 0) continue;

            foreach (var p in pos.EnumeratePossibilities())
            {
                if (used.Contains(p)) continue;

                var newUsed = used + p;
                var newTotal = total + p;

                if (newUsed.Count == sum.Length)
                {
                    if (newTotal == sum.Amount) return true;
                }
                else if (IsPossible(strategyUser, newUsed, cell, sum, newTotal, start + 1)) return true;
            }
        }

        return false;
    }
}