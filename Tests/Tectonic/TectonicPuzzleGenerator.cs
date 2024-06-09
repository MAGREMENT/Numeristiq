using Model.Core.Generators;
using Model.Tectonics;
using Model.Tectonics.Generator;

namespace Tests.Tectonic;

public class TectonicPuzzleGenerator
{
    private readonly IPuzzleGenerator<ITectonic> _generator =
        new RDRTectonicPuzzleGenerator(new BackTrackingFilledTectonicGenerator());

    [Test]
    public void Test()
    {
        var generated = _generator.Generate(10);

        foreach (var t in generated)
        {
            Console.WriteLine(t);
        }
    }
}