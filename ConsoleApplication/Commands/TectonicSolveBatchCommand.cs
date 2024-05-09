using System.Text;
using Model.Tectonics;

namespace ConsoleApplication.Commands;

public class TectonicSolveBatchCommand : Command
{
    private const int FileIndex = 0;
    private const int FeedbackIndex = 0;

    public override string Description => "Solves all the Tectonic's in a text file";

    public TectonicSolveBatchCommand() : base("SolveBatch", 
        new[]
        {
            new Argument("Text file containing the Tectonic's", ValueType.File)
        },
        new[]
        {
            new Option("--feedback", "Feedback for each Tectonic")
        }) { }
    
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        if (!interpreter.Instantiator.InstantiateTectonicSolver(out var solver)) return;
        
        using TextReader reader = new StreamReader((string)report.GetArgumentValue(FileIndex), Encoding.UTF8);

        int count = 1;
        int success = 0;
        while (reader.ReadLine() is { } line)
        {
            int commentStart = line.IndexOf('#');
            var s = commentStart == -1 ? line : line[..commentStart];

            var tectonic = TectonicTranslator.GuessFormat(s) switch
            {
                TectonicStringFormat.Code => TectonicTranslator.TranslateCodeFormat(s),
                TectonicStringFormat.Rd => TectonicTranslator.TranslateRdFormat(s),
                _ => null
            };
            if (tectonic is null)
            {
                Console.WriteLine($"Wrong format at line {count}");
                return;
            }
            
            solver.SetTectonic(tectonic);
            solver.Solve();

            var succeeded = solver.Tectonic.IsCorrect();
            if (succeeded) success++;
            if (report.IsOptionUsed(FeedbackIndex))
            {
                var status = succeeded ? "Ok !" : "Wrong !";
                Console.WriteLine($"#{count} {status}");
            }

            count++;
        }
        
        Console.WriteLine($"\nSolve Rate : {success}/{count - 1}");
    }
}