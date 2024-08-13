using Model.Binairos;

namespace Tests.Binairos;

public class BinairoBackTrackerTests
{
    [Test]
    public void Test()
    {
        var binairo = BinairoTranslator.TranslateLineFormat("6x6:1...1..1........0...0.0......1.0.0..");
        var copy = binairo.Copy();
        var expected = BinairoTranslator.TranslateLineFormat("6x6:100110011010001101110100010011101001");
        var backTracker = new BinairoBackTracker(binairo);

        var solutions = backTracker.Solutions();
        Assert.That(solutions, Has.Count.EqualTo(1));
        Assert.That(solutions[0].IsCorrect(), Is.True);

        backTracker.Count();
        Assert.That(binairo, Is.EqualTo(copy));
        
        Assert.That(backTracker.Fill(), Is.True);
        Assert.That(binairo, Is.EqualTo(expected));
    }

    [Test]
    public void MultiTest()
    {
        var binairo = BinairoTranslator.TranslateLineFormat("6x6:....11.....1.10.1...00..1...0...0...");
        var backTracker = new BinairoBackTracker(binairo);
        
        var c = backTracker.Count();
        Assert.That(c, Is.EqualTo(2));
    }

    [Test]
    public void IntactTest()
    {
        var binairo = BinairoTranslator.TranslateLineFormat("6x6:100110011010001101110100010011101001");
        var backTracker = new BinairoBackTracker();

        for (int row = 0; row < binairo.RowCount; row++)
        {
            for (int col = 0; col < binairo.ColumnCount; col++)
            {
                binairo[row, col] = 0;
                var copy = binairo.Copy();

                backTracker.Set(binairo);
                backTracker.Count();
                
                Assert.That(copy.SamePattern(binairo));
            }
        }
    }
}