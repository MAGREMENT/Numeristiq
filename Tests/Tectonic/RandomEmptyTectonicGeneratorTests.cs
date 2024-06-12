using Model.Tectonics.Generator;
using Model.Utility;

namespace Tests.Tectonic;

public class RandomEmptyTectonicGeneratorTests
{
    [Test]
    public void Test()
    {
        var generator = new RandomEmptyTectonicGenerator();

        for (int i = 0; i < 3; i++)
        {
            var t = generator.Generate();
            Console.WriteLine(t);
            
            Assert.That(BackTracking.Fill(t, new TectonicPossibilitiesGiver(t), 1), Has.Count.EqualTo(1));
        }
    }
}