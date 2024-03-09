using Model.Helpers.Changes;
using Model.Helpers.Changes.Buffers;
using Model.Sudoku;

namespace ConsoleApplication.Commands;

public class SudokuSolveCommand : Command
{
    private const int StringIndex = 0;
    private const int PathIndex = 1;
    
    public override string Description => "Solves a Sudoku";
    
    public SudokuSolveCommand() : base("SudokuSolve",
        new Option("-s", "Sudoku string", OptionValueRequirement.Mandatory, OptionValueType.String),
        new Option("-p", "Show path"))
    {
    }
    
    public override void Execute(IReadOnlyArgumentInterpreter interpreter, IReadOnlyOptionsReport report)
    {
        if (!report.IsUsed(StringIndex))
        {
            Console.WriteLine("No Sudoku given");
            return;
        }
        
        if (!interpreter.Instantiator.InstantiateSudokuSolver(out var solver)) return;

        var oldBuffer = solver.ChangeBuffer;
        if (report.IsUsed(PathIndex)) solver.ChangeBuffer = new LogManagedChangeBuffer(solver);
        else solver.ChangeBuffer = new FastChangeBuffer(solver);

        var sudoku = SudokuTranslator.TranslateLineFormat((string)report.GetValue(StringIndex)!);
        
        Console.WriteLine($"Before :\n{sudoku}");

        solver.SetSudoku(sudoku);
        solver.Solve();
        
        Console.WriteLine($"After : \n{sudoku}");

        if (report.IsUsed(PathIndex))
        {
            Console.WriteLine("\nPath :");
            foreach (var log in solver.Logs)
            {
                var explanation = log.Explanation is null ? "None" : log.Explanation.FullExplanation();
                Console.WriteLine($"{log.Id}. {log.Title}\nDescription : {log.Description}\nChanges :" +
                                  $" {ChangeReportHelper.ChangesToString(log.Changes)}\nExplanation : {explanation}");
            }
        }
        
        solver.ChangeBuffer = oldBuffer;
    }
}