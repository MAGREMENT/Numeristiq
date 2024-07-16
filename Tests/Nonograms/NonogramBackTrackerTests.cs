using Model.Core.BackTracking;
using Model.Nonograms;
using Model.Nonograms.Solver;

namespace Tests.Nonograms;

public class NonogramBackTrackerTests
{
    [Test]
    public void CountTest()
    {
        var nonogram = NonogramTranslator.TranslateLineFormat("4-3-2.1-1-2::1.1-1.3-2-3-2");
        var backTracker = new NaiveNonogramBackTracker(nonogram, ConstantAvailabilityChecker.Instance);

        var count = backTracker.Count();
        Assert.That(count, Is.EqualTo(1));

        nonogram = NonogramTranslator.TranslateLineFormat("3.1-2.1-1.1-2.1-1::2.1-4-1.1-1-2.1");
        backTracker.Set(nonogram);

        count = backTracker.Count();
        Assert.That(count, Is.GreaterThan(1));
        Console.WriteLine(count);
    }

    [Test]
    public void FillTest()
    {
        var nonogram = NonogramTranslator.TranslateLineFormat("1.2-3-1.2-1.2-2.2-2::3-1.2-1.1-3-2.2-2.1");
        var backTracker = new NaiveNonogramBackTracker(nonogram, ConstantAvailabilityChecker.Instance);

        var s = backTracker.Solutions();
        Console.WriteLine(s[0]);
        Assert.That(s[0].IsCorrect(), Is.True);

        backTracker.Fill();
        Console.WriteLine(backTracker.Current);
        Assert.That(backTracker.Current.IsCorrect(), Is.True);
    }
}