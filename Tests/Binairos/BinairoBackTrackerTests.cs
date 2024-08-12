using Model.Binairos;

namespace Tests.Binairos;

public class BinairoBackTrackerTests
{
    [Test]
    public void Test()
    {
        var binairo = BinairoTranslator.TranslateLineFormat("6x6:1...1..1........0...0.0......1.0.0..");
        var expected = BinairoTranslator.TranslateLineFormat("6x6:100110011010001101110100010011101001");
        var backTracker = new BinairoBackTracker(binairo);

        var solutions = backTracker.Solutions();
        Assert.That(solutions, Has.Count.EqualTo(1));
        Assert.That(solutions[0].IsCorrect(), Is.True);
        
        Assert.That(backTracker.Fill(), Is.True);
        Assert.That(binairo.SamePattern(expected), Is.True);
    }

    [Test]
    public void MultiTest()
    {
        var binairo = BinairoTranslator.TranslateLineFormat("6x6:....11.....1.10.1...00..1...0...0...");
        var backTracker = new BinairoBackTracker(binairo);
        
        var c = backTracker.Count();
        Assert.That(c, Is.EqualTo(2));
    }
}