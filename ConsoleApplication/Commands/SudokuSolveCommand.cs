using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Changes.Buffers;
using Model.Helpers.Highlighting;
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
            new Option("-p", "Show solving path")
        }) { }
    
    public override void Execute(IReadOnlyArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        if (!interpreter.Instantiator.InstantiateSudokuSolver(out var solver)) return;

        var oldBuffer = solver.ChangeBuffer;
        if (report.IsOptionUsed(PathIndex)) solver.ChangeBuffer = new LogManagedChangeBuffer<IUpdatableSudokuSolvingState, ISudokuHighlighter>(solver);
        else solver.ChangeBuffer = new FastChangeBuffer<IUpdatableSudokuSolvingState, ISudokuHighlighter>(solver);

        var sudoku = SudokuTranslator.TranslateLineFormat((string)report.GetArgumentValue(StringIndex));
        
        Console.WriteLine($"Before :\n{sudoku}");

        solver.SetSudoku(sudoku);
        solver.Solve();
        
        Console.WriteLine($"After : \n{sudoku}");

        if (report.IsOptionUsed(PathIndex))
        {
            Console.WriteLine("\nPath :");
            foreach (var log in solver.LogManager.Logs)
            {
                var explanation = log.Explanation is null ? "None" : log.Explanation.FullExplanation();
                Console.WriteLine($"{log.Id}. {log.Title}\nDescription : {log.Description}\nChanges :" +
                                  $" {ChangeReportHelper.ChangesToString(log.Changes)}\nExplanation : {explanation}");
            }
        }
        
        solver.ChangeBuffer = oldBuffer;
    }
}