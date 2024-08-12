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
        
        Assert.That(backTracker.Fill(), Is.True);
        Assert.That(binairo.SamePattern(expected), Is.True);
    }
}