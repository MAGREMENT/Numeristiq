using System.Text;
using Model.Core;
using Model.Core.Trackers;
using Model.Sudokus;
using Model.Sudokus.Solver;

namespace ConsoleApplication.Commands;

public class SudokuSolveBatchCommand : Command
{
    private const int FileIndex = 0;
    private const int FeedbackIndex = 0;
    private const int UnorderedIndex = 1;
    private const int FailsIndex = 2;
    private const int InstancesIndex = 3;
    
    public override string Description => "Solves all the Sudoku's in a text file";
    
    public SudokuSolveBatchCommand() : base("SolveBatch",
        new[]
        {
            new Argument("Text file containing the Sudoku's", ValueType.File)
        },
        new []
        {
            new Option("--feedback", "Gives feedback for each Sudoku"),
            new Option("-u", "Sets all strategies instance handling to unordered all"),
            new Option("--list-fails", "Lists all solver fails"),
            new Option("--list-instances", "Lists all Sudoku's that presented the strategy in their solution path",
                ValueRequirement.Mandatory, ValueType.String)
        }) { }
    
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var solver = interpreter.Instantiator.InstantiateSudokuSolver();
        //solver.FastMode = true; TODO test

        if (report.IsOptionUsed(UnorderedIndex))
        {
            foreach (var s in solver.StrategyManager.Strategies)
            {
                s.InstanceHandling = InstanceHandling.UnorderedAll;
            }
        }
        
        var statistics = new SudokuStatisticsTracker();
        statistics.AttachTo(solver);

        if (report.IsOptionUsed(FeedbackIndex)) statistics.SolveDone += OnSolveDone;

        List<string> fails = new();
        List<string> instances = new();
        UsedStrategiesTracker<SudokuStrategy, ISudokuSolveResult>? usedTracker = null;

        if (report.IsOptionUsed(InstancesIndex))
        {
            usedTracker = new UsedStrategiesTracker<SudokuStrategy, ISudokuSolveResult>();
            usedTracker.AttachTo(solver);
        }
        
        using TextReader reader = new StreamReader((string)report.GetArgumentValue(FileIndex), Encoding.UTF8);

        while (reader.ReadLine() is { } line)
        {
            int commentStart = line.IndexOf('#');
            var s = commentStart == -1 ? line : line[..commentStart];
            
            solver.SetSudoku(SudokuTranslator.TranslateLineFormat(s));
            solver.Solve();

            if (report.IsOptionUsed(FailsIndex) && solver.IsWrong())
            {
                fails.Add(s);
            }

            if (usedTracker is not null && usedTracker.WasUsed((string)report.GetOptionValue(InstancesIndex)!))
            {
                instances.Add(s);
            }
        }

        Console.WriteLine(statistics);

        if (report.IsOptionUsed(FailsIndex))
        {
            if(fails.Count == 0) Console.WriteLine("\nNo fail detected");
            else
            {
                Console.WriteLine($"\n{fails.Count} fails detected");
                for (int i = 0; i < fails.Count; i++)
                {
                    Console.WriteLine($"   {i + 1}: {fails[i]}");
                }
            }
        }
        
        if (report.IsOptionUsed(InstancesIndex))
        {
            if(instances.Count == 0) Console.WriteLine("\nNo instance detected");
            else
            {
                Console.WriteLine($"\n{instances.Count} instances detected");
                for (int i = 0; i < instances.Count; i++)
                {
                    Console.WriteLine($"   {i + 1}: {instances[i]}");
                }
            }
        }
    }

    private static void OnSolveDone(ISudokuSolveResult result, int count)
    {
        Console.Write($"#{count} ");
        if(result.Sudoku.IsCorrect()) Console.WriteLine("Ok !");
        else
        {
            Console.Write(result.IsWrong() ? "Solver failed" : "Solver did not find solution");
            Console.WriteLine($" => {SudokuTranslator.TranslateLineFormat(result.StartState, SudokuLineFormatEmptyCellRepresentation.Points)}");
        }
    }
}