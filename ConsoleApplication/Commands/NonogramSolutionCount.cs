using ConsoleApplication.Commands.Abstracts;
using Model.Core.BackTracking;
using Model.Nonograms;
using Model.Nonograms.BackTrackers;
using Model.Nonograms.Solver;

namespace ConsoleApplication.Commands;

public class NonogramSolutionCount : SolutionCountCommand<Nonogram, IAvailabilityChecker>
{
    public NonogramSolutionCount() : base("Nonogram")
    {
    }

    protected override BackTracker<Nonogram, IAvailabilityChecker> BackTracker { get; }
        = new NaiveNonogramBackTracker(new Nonogram(), ConstantAvailabilityChecker.Instance);
    protected override void SetBackTracker(string s)
    {
        BackTracker.Set(NonogramTranslator.TranslateLineFormat(s));
    }

    protected override string ToString(Nonogram puzzle)
    {
        return puzzle.ToString();
    }
}