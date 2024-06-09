using Model.Tectonics.Generator;

namespace Tests.Tectonic;

public class RandomEmptyTectonicGeneratorTests
{
    [Test]
    public void Test()
    {
        var generator = new RandomEmptyTectonicGenerator();

        var t = generator.Generate();
        Console.WriteLine(t);
    }
}