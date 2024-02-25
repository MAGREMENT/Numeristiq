using Model.Sudoku;

namespace ConsoleApplication.Commands;

public class SudokuSolveCommand : Command
{
    private const int StringIndex = 0;
    
    public override string Description => "Solves a Sudoku";
    
    public SudokuSolveCommand() : base("SudokuSolve",
        new Option("-s", "Sudoku string", OptionValueRequirement.Mandatory, OptionValueType.String))
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

        var sudoku = SudokuTranslator.TranslateLineFormat((string)report.GetValue(StringIndex)!);
        
        Console.WriteLine($"Before :\n{sudoku}");

        solver.SetSudoku(sudoku);
        solver.Solve();
        
        Console.WriteLine($"After : \n{sudoku}");
    }
}