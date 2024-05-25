using Model.Tectonics;

namespace ConsoleApplication.Commands;

public class TectonicSolveCommand : Command
{
    private const int StringIndex = 0;
    
    public override string Description => "Solves a Tectonic";
    
    public TectonicSolveCommand() : base("Solve",
        new Argument("Tectonic string", ValueType.String)) { }
    
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var solver = interpreter.Instantiator.InstantiateTectonicSolver();
        var tectonic = TectonicTranslator.TranslateCodeFormat((string)report.GetArgumentValue(StringIndex));
        
        Console.WriteLine($"Before :\n{tectonic}");

        solver.SetTectonic(tectonic);
        solver.Solve();
        
        Console.WriteLine($"After : \n{tectonic}");
    }
}