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
    public void UniqueTest() //TODO fix
    {
        var generator = new RandomCrossSumGenerator
        {
            KeepUniqueness = true,
            KeepSymmetry = false
        };

        var cs = generator.Generate();
        Console.WriteLine(cs.ToString());
    }
}