using Model.Tectonics;
using Model.Utility;

namespace ConsoleApplication.Commands;

public class TectonicSolutionCountCommand : Command
{
    private const int SudokuIndex = 0;

    public TectonicSolutionCountCommand() : base("SolutionCount", 
        new Argument("Tectonic string", ValueType.String))
    {
    }

    public override string Description => "Counts the number of solutions for a given Sudoku";
    public override void Execute(IReadOnlyArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var tectonic = TectonicTranslator.TranslateRdFormat((string)report.GetArgumentValue(SudokuIndex));

        var result = BackTracking.Fill(tectonic, new ITectonicPossibilitiesGiver(tectonic),
            int.MaxValue);
        
        Console.WriteLine($"Number of solutions : {result.Count}\n");

        foreach (var t in result)
        {
            Console.WriteLine(TectonicTranslator.TranslateRdFormat(t));
        }
    }
}