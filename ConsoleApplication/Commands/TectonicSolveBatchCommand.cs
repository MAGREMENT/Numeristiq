using System.Text;
using Model.Tectonic;

namespace ConsoleApplication.Commands;

public class TectonicSolveBatchCommand : Command
{
    private const int FileIndex = 0;
    private const int FeedbackIndex = 1;

    public override string Description => "Solves all the Tectonic's in a text file";

    public TectonicSolveBatchCommand() : base("TectonicSolveBatch", 
        new Option("-f", "Text file containing the Tectonic's", OptionValueRequirement.Mandatory, OptionValueType.File),
        new Option("--feedback", "Feedback for each Tectonic"))
    {
    }

    
    public override void Execute(IReadOnlyArgumentInterpreter interpreter, IReadOnlyOptionsReport report)
    {
        if (!report.IsUsed(FileIndex))
        {
            Console.WriteLine("No file specified");
            return;
        }

        if (!interpreter.Instantiator.InstantiateTectonicSolver(out var solver)) return;
        
        using TextReader reader = new StreamReader((string)report.GetValue(FileIndex)!, Encoding.UTF8);

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
            if (report.IsUsed(FeedbackIndex))
            {
                var status = succeeded ? "Ok !" : "Wrong !";
                Console.WriteLine($"#{count} {status}");
            }

            count++;
        }
        
        Console.WriteLine($"\nSolve Rate : {success}/{count - 1}");
    }
}