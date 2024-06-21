using Model.Core.Generators;
using Model.Tectonics;
using Model.Tectonics.Generator;

namespace Tests.Tectonics;

public class TectonicPuzzleGeneratorTests
{
    private readonly IPuzzleGenerator<ITectonic> _generator =
        new RDRTectonicPuzzleGenerator(new RandomLayoutBackTrackingFilledTectonicGenerator());

    [Test]
    public void Test()
    {
        var generated = _generator.Generate(1);

        foreach (var t in generated)
        {
            Console.WriteLine(t);
        }
    }
}