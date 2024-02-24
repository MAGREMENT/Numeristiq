using Model.Tectonic;

namespace ConsoleApplication.Commands;

public class TectonicSolveCommand : Command
{
    private const int StringIndex = 0;
    
    public override string Description => "Solves a Tectonic";
    
    public TectonicSolveCommand() : base("TectonicSolve",
        new Option("-s", "Tectonic string", OptionValueRequirement.Mandatory, OptionValueType.String))
    {
    }
    
    public override void Execute(IReadOnlyArgumentInterpreter interpreter, IReadOnlyOptionsReport report)
    {
        if (!report.IsUsed(StringIndex))
        {
            Console.WriteLine("No tectonic given");
            return;
        }
        
        if (!interpreter.Instantiator.InstantiateTectonicSolver(out var solver)) return;

        var tectonic = TectonicTranslator.TranslateLineFormat((string)report.GetValue(StringIndex)!);
        
        Console.WriteLine($"Before :\n{tectonic}");

        solver.SetTectonic(tectonic);
        solver.Solve();
        
        Console.WriteLine($"After : \n{tectonic}");
    }
}