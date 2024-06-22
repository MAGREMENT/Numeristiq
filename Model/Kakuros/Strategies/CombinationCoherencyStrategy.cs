using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Kakuros.Strategies;

public class CombinationCoherencyStrategy : Strategy<IKakuroSolverData>
{
    public CombinationCoherencyStrategy() : base("Combination Coherency", StepDifficulty.Medium, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(IKakuroSolverData solverData)
    {
        foreach (var sum in solverData.Kakuro.Sums)
        {
            var used = new ReadOnlyBitSet16();
            int total = 0;
            foreach (var cell in sum)
            {
                var n = solverData[cell.Row, cell.Column];
                if (n != 0)
                {
                    used += n;
                    total += n;
                }
            }

            if (used.Count == sum.Length) continue;

            foreach (var cell in sum)
            {
                var possibilities = solverData.PossibilitiesAt(cell);
                if (possibilities.Count == 0) continue;

                foreach (var p in possibilities.EnumeratePossibilities())
                {
                    if (IsPossible(solverData, used + p, cell, sum, total + p, 0)) continue;

                    solverData.ChangeBuffer.ProposePossibilityRemoval(p, cell);
                    solverData.ChangeBuffer.Commit(
                        DefaultNumericChangeReportBuilder<IUpdatableNumericSolvingState, INumericSolvingStateHighlighter>.Instance);
                }
            }
        }
    }

    private static bool IsPossible(IKakuroSolverData solverData, ReadOnlyBitSet16 used, 
        Cell cell, IKakuroSum sum, int total, int start)
    {
        for (; start < sum.Length; start++)
        {
            var c = sum[start];
            if (c == cell) continue;

            var pos = solverData.PossibilitiesAt(c);
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
                else if (IsPossible(solverData, newUsed, cell, sum, newTotal, start + 1)) return true;
            }
        }

        return false;
    }
}