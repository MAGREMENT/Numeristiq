using Model.Binairos;

namespace Tests.Binairos;

public class RDRBinairoPuzzleGeneratorTests
{
    [Test]
    public void Test()
    {
        const int count = 10;
        var filled = new FilledBinairoPuzzleGenerator();
        var generator = new RDRBinairoPuzzleGenerator(filled);
        var backTracker = new BinairoBackTracker();

        filled.Randomizer.MaxRowCount = 6;
        filled.Randomizer.MinColumnCount = 6;
        filled.Randomizer.MaxColumnCount = 6;
        filled.Randomizer.MinRowCount = 6;

        for (int i = 0; i < count; i++)
        {
            var b = generator.Generate();
            backTracker.Set(b);
            
            Console.WriteLine(BinairoTranslator.TranslateLineFormat(b));
            Assert.That(backTracker.Count(), Is.EqualTo(1));
        }
    }
}