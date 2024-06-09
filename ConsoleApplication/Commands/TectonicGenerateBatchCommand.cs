using Model.Core.Generators;
using Model.Tectonics;
using Model.Tectonics.Generator;

namespace ConsoleApplication.Commands;

public class TectonicGenerateBatchCommand : Command
{
    private const int CountIndex = 0;
    
    public override string Description => "Generate a determined amount of Sudoku's";
    
    private readonly IPuzzleGenerator<ITectonic> _generator = new RDRTectonicPuzzleGenerator(
        new BackTrackingFilledTectonicGenerator(new RandomEmptyTectonicGenerator()));
    
    //TODO rowcount & columncount
    public TectonicGenerateBatchCommand() : base("GenerateBatch",
        new Option("-c", "Count", ValueRequirement.Mandatory, ValueType.Int))
    {
    }
    
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var count = (int)(report.GetOptionValue(CountIndex) ?? 1);
        
        Console.WriteLine("Started generating...");
        var start = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        for (int i = 0; i < count; i++)
        {
            var generated = _generator.Generate();
            Console.WriteLine(TectonicTranslator.TranslateRdFormat(generated));
        }
        
        Console.WriteLine($"Finished generating in {Math.Round((double)(DateTimeOffset.Now.ToUnixTimeMilliseconds() - start) / 1000, 4)}s");
    }
}