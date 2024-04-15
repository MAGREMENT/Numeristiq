using Model.Sudoku;
using Model.Sudoku.Generator;
using Model.Sudoku.Solver.StrategiesUtility;

namespace ConsoleApplication.Commands;

public class SudokuSolutionCountCommand : Command
{
    private const int SudokuIndex = 0;

    public SudokuSolutionCountCommand() : base("SolutionCount", 
        new Argument("Sudoku string", ValueType.String))
    {
    }

    public override string Description => "Counts the number of solutions for a given Sudoku";
    public override void Execute(IReadOnlyArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var sudoku = SudokuTranslator.TranslateLineFormat((string)report.GetArgumentValue(SudokuIndex));

        var result = BackTracking.Fill(sudoku, ConstantPossibilitiesGiver.Instance, int.MaxValue);
        
        Console.WriteLine($"Number of solutions : {result.Length}\n");

        foreach (var s in result)
        {
            Console.WriteLine(SudokuTranslator.TranslateLineFormat(s, SudokuLineFormatEmptyCellRepresentation.Points));
        }
    }
}