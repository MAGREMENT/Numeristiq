using Model.Core.Changes;
using Model.Sudokus;

namespace ConsoleApplication.Commands;

public class SudokuSolveCommand : Command
{
    private const int StringIndex = 0;
    private const int PathIndex = 0;
    
    public override string Description => "Solves a Sudoku";
    
    public SudokuSolveCommand() : base("Solve",
        new []
        {
            new Argument("Sudoku string", ValueType.String)
        },
        new[]
        {
            new Option("-p", "Shows solving path")
        }) { }
    
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var solver = interpreter.Instantiator.InstantiateSudokuSolver();

        if (!report.IsOptionUsed(PathIndex)) solver.FastMode = true;

        var sudoku = SudokuTranslator.TranslateLineFormat((string)report.GetArgumentValue(StringIndex));
        
        Console.WriteLine($"Before :\n{sudoku}");

        solver.SetSudoku(sudoku);
        solver.Solve();
        
        Console.WriteLine($"After : \n{sudoku}");

        if (report.IsOptionUsed(PathIndex))
        {
            Console.WriteLine("\nPath :");
            foreach (var log in solver.Steps)
            {
                var explanation = log.Explanation is null ? "None" : log.Explanation.FullExplanation();
                Console.WriteLine($"{log.Id}. {log.Title}\nDescription : {log.Description}\nChanges :" +
                                  $" {ChangeReportHelper.ChangesToString(log.Changes)}\nExplanation : {explanation}");
            }
        }
    }
}