using Model.Core.BackTracking;
using Model.Nonograms;
using Model.Nonograms.BackTrackers;
using Tests.Utility;

namespace Tests.Nonograms;

public class NonogramBackTrackerTests
{
    [Test]
    public void CountTest()
    {
        CountTest(new NaiveNonogramBackTracker());
        CountTest(new SpareSpaceNonogramBackTracker());
    }
    
    private static void CountTest(BackTracker<Nonogram, IAvailabilityChecker> backTracker)
    {
        var nonogram = NonogramTranslator.TranslateLineFormat("4-3-2.1-1-2::1.1-1.3-2-3-2");
        backTracker.Set(nonogram);

        var count = backTracker.Count();
        Assert.That(count, Is.EqualTo(1));

        nonogram = NonogramTranslator.TranslateLineFormat("3.1-2.1-1.1-2.1-1::2.1-4-1.1-1-2.1");
        backTracker.Set(nonogram);

        count = backTracker.Count();
        Assert.That(count, Is.EqualTo(2));
        Console.WriteLine(count);
    }

    [Test]
    public void FillTest()
    {
        FillTest(new NaiveNonogramBackTracker());
        FillTest(new SpareSpaceNonogramBackTracker());
    }

    private static void FillTest(BackTracker<Nonogram, IAvailabilityChecker> backTracker)
    {
        var nonogram = NonogramTranslator.TranslateLineFormat("1.2-3-1.2-1.2-2.2-2::3-1.2-1.1-3-2.2-2.1");
        var copy = nonogram.Copy();
        backTracker.Set(nonogram);

        var s = backTracker.Solutions();
        Console.WriteLine(s[0]);
        Assert.That(s[0].IsCorrect(), Is.True);
        Assert.That(backTracker.Current.SamePattern(copy), Is.True);

        backTracker.Fill();
        Console.WriteLine(backTracker.Current);
        Assert.That(backTracker.Current.IsCorrect(), Is.True);
        Assert.That(backTracker.Current.SamePattern(copy), Is.False);
    }

    [Test]
    public void SolutionsNonEmptyTest()
    {
        SolutionsNonEmptyTest(new NaiveNonogramBackTracker());
        SolutionsNonEmptyTest(new SpareSpaceNonogramBackTracker());
    }
    
    private static void SolutionsNonEmptyTest(BackTracker<Nonogram, IAvailabilityChecker> backTracker)
    {
        var nonogram = NonogramTranslator.TranslateLineFormat(
            "2.3-1.1-1.3-2.2-3.1-1.1.1-1::1.1.2-1.2-5-1.1-4-1.1.1-2::0.5-1.0-1.3-2.2-2.3-2.4-4.0-4.1-4.2-4.3-5.0");
        var copy = nonogram.Copy();
        backTracker.Set(nonogram);
        
        var s = backTracker.Solutions();
        Console.WriteLine(backTracker.Current);
        Assert.That(s[0].IsCorrect(), Is.True);
        Assert.That(copy.SamePattern(nonogram), Is.True);
    }

    /*
     #1 NaiveNonogramBackTracker
       Total: 1,9152618 s
       Average: 19,152618 ms
       Minimum: 16,6277 ms on try #70
       Maximum: 39,0593 ms on try #95
       Ignored: 1

    #2 SpareSpaceNonogramBackTracker
       Total: 671,5127 ms
       Average: 6,715127 ms
       Minimum: 5,5485 ms on try #50
       Maximum: 13,0547 ms on try #37
       Ignored: 1
    */
    [Test]
    public void ImplementationTest()
    {
        var nonogram = NonogramTranslator.TranslateLineFormat("1.2-3-1.2-1.2-2.2-2::3-1.2-1.1-3-2.2-2.1");
        var expected = NonogramTranslator.TranslateLineFormat(
            "1.2-3-1.2-1.2-2.2-2::3-1.2-1.1-3-2.2-2.1::0.2-0.3-0.4-1.0-1.4-1.5-2.2-2.5-3.1-3.2-3.3-4.0-4.1-4.3-4.4-5.0-5.1-5.4");
        
        ImplementationSpeedComparator.Compare<BackTracker<Nonogram, IAvailabilityChecker>>(bt =>
        {
            bt.Set(nonogram);
            var solution = bt.Solutions()[0];
            Assert.That(solution.SamePattern(expected));
        }, 100, new NaiveNonogramBackTracker(), new SpareSpaceNonogramBackTracker());
    }
}