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

    public override void Apply(IKakuroSolverData solverData)
    {
        foreach (var sum in solverData.Kakuro.Sums)
        {
            var total = 0;
            int withSolutions = 1;
            foreach (var cell in sum)
            {
                var n = solverData[cell.Row, cell.Column];
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
                if (solverData[cell.Row, cell.Column] != 0) continue;

                var possibilities = solverData.PossibilitiesAt(cell);
                foreach (var p in possibilities.EnumeratePossibilities())
                {
                    if (total + p + min > sum.Amount || total + p + max < sum.Amount)
                    {
                        solverData.ChangeBuffer.ProposePossibilityRemoval(p, cell);
                        solverData.ChangeBuffer.Commit(
                            DefaultNumericChangeReportBuilder<IUpdatableNumericSolvingState, INumericSolvingStateHighlighter>.Instance);
                    }
                }
            }
        }
    }
}