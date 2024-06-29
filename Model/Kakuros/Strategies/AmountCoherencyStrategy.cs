using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver;
using Model.Utility.BitSets;

namespace Model.Kakuros.Strategies;

public class AmountCoherencyStrategy : Strategy<IKakuroSolverData>
{
    public AmountCoherencyStrategy() : base("Amount Coherency", StepDifficulty.Easy, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(IKakuroSolverData data)
    {
        foreach (var sum in data.Kakuro.Sums)
        {
            var total = 0;
            int withSolutions = 1;
            foreach (var cell in sum)
            {
                var n = data[cell.Row, cell.Column];
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
                if (data[cell.Row, cell.Column] != 0) continue;

                var possibilities = data.PossibilitiesAt(cell);
                foreach (var p in possibilities.EnumeratePossibilities())
                {
                    if (total + p + min > sum.Amount || total + p + max < sum.Amount)
                    {
                        data.ChangeBuffer.ProposePossibilityRemoval(p, cell);
                        data.ChangeBuffer.Commit(
                            DefaultNumericChangeReportBuilder<INumericSolvingState, INumericSolvingStateHighlighter>.Instance);
                    }
                }
            }
        }
    }
}