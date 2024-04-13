using System.Text;
using Model.Sudoku;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Trackers;

namespace ConsoleApplication.Commands;

public class SudokuSolveBatchCommand : Command
{
    private const int FileIndex = 0;
    private const int FeedbackIndex = 1;
    private const int WaitForAllIndex = 2;
    
    public override string Description => "Solves all the Sudoku's in a text file";
    
    public SudokuSolveBatchCommand() : base("SudokuSolveBatch",
        new Option("-f", "Text file containing the Sudoku's", OptionValueRequirement.Mandatory, OptionValueType.File),
        new Option("--feedback", "Feedback for each Sudoku"),
        new Option("-u", "Set all strategies instance handling to unordered all"))
    {
    }
    
    public override void Execute(IReadOnlyArgumentInterpreter interpreter, IReadOnlyOptionsReport report)
    {
        if (!report.IsUsed(FileIndex))
        {
            Console.WriteLine("No file specified");
            return;
        }
        
        if (!interpreter.Instantiator.InstantiateSudokuSolver(out var solver)) return;

        if (report.IsUsed(WaitForAllIndex))
        {
            foreach (var s in solver.StrategyManager.Strategies)
            {
                s.InstanceHandling = InstanceHandling.UnorderedAll;
            }
        }
        
        var statistics = new StatisticsTracker();
        solver.AddTracker(statistics);

        if (report.IsUsed(FeedbackIndex)) statistics.NotifySolveDone = true;
        
        using TextReader reader = new StreamReader((string)report.GetValue(FileIndex)!, Encoding.UTF8);

        while (reader.ReadLine() is { } line)
        {
            int commentStart = line.IndexOf('#');
            var s = commentStart == -1 ? line : line[..commentStart];
            
            solver.SetSudoku(SudokuTranslator.TranslateLineFormat(s));
            solver.Solve();
        }

        Console.WriteLine(statistics);
    }
}