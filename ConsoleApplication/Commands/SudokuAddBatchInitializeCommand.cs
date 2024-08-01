using System.Text;
using Model.Core;
using Model.Core.Trackers;
using Model.Sudokus;

namespace ConsoleApplication.Commands;

public class SudokuAddBatchInitializeCommand : Command
{
    private const int FileIndex = 0;
    
    public SudokuAddBatchInitializeCommand() : base("AddBatch", 
        new Argument("File name", ValueType.File))
    {
    }

    public override string Description => "Evaluates and adds all the Sudoku's in a text file";
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var repo = interpreter.Instantiator.InstantiateBankRepository();
        var solver = interpreter.Instantiator.InstantiateSudokuSolver();
        solver.FastMode = true;
        
        var hardest = new HardestStrategyTracker();
        hardest.AttachTo(solver);
        
        using TextReader reader = new StreamReader((string)report.GetArgumentValue(FileIndex), Encoding.UTF8);
        List<(Sudoku, Difficulty)> toAdd = new();
        int count = 0;
        while (reader.ReadLine() is { } line)
        {
            var commentStart = line.IndexOf('#');
            var s = commentStart == -1 ? line : line[..commentStart];
            var sudoku = SudokuTranslator.TranslateLineFormat(s);
            var copy = sudoku.Copy();

            solver.SetSudoku(sudoku);
            solver.Solve();
            Console.WriteLine($"#{++count} evaluated");

            if (hardest.Hardest is not null) toAdd.Add((copy, hardest.Hardest.Difficulty));
        }

        Console.WriteLine("Started adding to the repository");
        try
        {
            repo.Add(toAdd);
            Console.WriteLine("Operation done successfully");
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
        }
        
        hardest.Detach();
    }
}