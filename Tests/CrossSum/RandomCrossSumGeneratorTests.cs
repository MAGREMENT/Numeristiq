using Model.CrossSums;

namespace Tests.CrossSum;

public class RandomCrossSumGeneratorTests
{
    [Test]
    public void NonUniqueTest()
    {
        var generator = new RandomCrossSumGenerator
        {
            KeepUniqueness = false,
            KeepSymmetry = false
        };

        var cs = generator.Generate();
        Console.WriteLine(cs.ToString());
    }
    
    [Test]
    public void UniqueTest()
    {
        const int size = 8;
        var generator = new RandomCrossSumGenerator
        {
            KeepUniqueness = true,
            KeepSymmetry = false
        };

        generator.Randomizer.MinColumnCount = size;
        generator.Randomizer.MaxColumnCount = size;
        generator.Randomizer.MinRowCount = size;
        generator.Randomizer.MaxRowCount = size;

        var cs = generator.Generate();
        Console.WriteLine(cs.ToString());
        Console.WriteLine('\n');
        Console.WriteLine(CrossSumTranslator.Translate(cs));
    }
}