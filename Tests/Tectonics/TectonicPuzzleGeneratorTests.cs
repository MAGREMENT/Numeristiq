using Model.Tectonics.Generator;

namespace Tests.Tectonics;

public class TectonicPuzzleGeneratorTests
{
    private readonly RDRTectonicPuzzleGenerator _generator = new(new RandomLayoutBackTrackingFilledTectonicGenerator());

    [Test]
    public void Test()
    {
        /*var generated = _generator.Generate(1); TODO

        foreach (var t in generated)
        {
            Console.WriteLine(t);
        }*/
    }
}