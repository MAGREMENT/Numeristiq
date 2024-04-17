using System.Text;
using Model.Sudokus;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Trackers;

namespace ConsoleApplication.Commands;

public class SudokuSolveBatchCommand : Command
{
    private const int FileIndex = 0;
    private const int FeedbackIndex = 0;
    private const int WaitForAllIndex = 0;
    
    public override string Description => "Solves all the Sudoku's in a text file";
    
    public SudokuSolveBatchCommand() : base("SolveBatch",
        new[]
        {
            new Argument("Text file containing the Sudoku's", ValueType.File)
        },
        new []
        {
            new Option("--feedback", "Feedback for each Sudoku"),
            new Option("-u", "Set all strategies instance handling to unordered all")
        }) { }
    
    public override void Execute(IReadOnlyArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        if (!interpreter.Instantiator.InstantiateSudokuSolver(out var solver)) return;

        if (report.IsOptionUsed(WaitForAllIndex))
        {
            foreach (var s in solver.StrategyManager.Strategies)
            {
                s.InstanceHandling = InstanceHandling.UnorderedAll;
            }
        }
        
        var statistics = new StatisticsTracker();
        solver.AddTracker(statistics);

        if (report.IsOptionUsed(FeedbackIndex)) statistics.NotifySolveDone = true;
        
        using TextReader reader = new StreamReader((string)report.GetArgumentValue(FileIndex), Encoding.UTF8);

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