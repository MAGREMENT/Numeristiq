using Model.Sudoku;
using Model.Sudoku.Generator;
using Model.Sudoku.Solver;
using Model.Utility;
using Repository;

namespace SudokuGenerator;

public static class Program
{
    public static void Main(string[] args)
    {
        var r = SetUpArgsReader();
        var argResult = r.Read(args);

        var count = int.Parse(argResult.GetValue("c"));
        var generator = new RCRSudokuPuzzleGenerator(new BackTrackingFilledSudokuGenerator());

        List<GeneratedSudoku> puzzles = new(count);
        
        var repo = new SudokuStrategiesJSONRepository("strategies.json");
        if (!repo.Initialize(false))
        {
            Console.WriteLine("Exception while initializing repository : ");
            return;
        }

        var solver = new SudokuSolver{
            ChangeManagement = ChangeManagement.Fast
        };
        solver.StrategyManager.AddStrategies(repo.Download());

        var ratings = new RatingTracker(solver);
        var hardest = new HardestStrategyTracker(solver);

        Console.WriteLine("Started generating...");
        var start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        var generated = generator.Generate(count);
        Console.WriteLine($"Finished generating in {Math.Round((double)(DateTimeOffset.Now.ToUnixTimeMilliseconds() - start) / 1000, 4)}s");
        
        Console.WriteLine("Started evaluating...");
        start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        foreach (var p in generated)
        {
            solver.SetSudoku(p.Copy());
            solver.Solve();
            puzzles.Add(new GeneratedSudoku(SudokuTranslator.TranslateLineFormat(p, SudokuTranslationType.Points),
                ratings.Rating, hardest.Hardest.Name));
            
            ratings.Clear();
            hardest.Clear();
        }
        Console.WriteLine($"Finished evaluating in {Math.Round((double)(DateTimeOffset.Now.ToUnixTimeMilliseconds() - start) / 1000, 4)}s");

        if (argResult.Contains("s"))
        {
            puzzles.Sort((s1, s2) => (int)(s2.Rating * 1000 - s1.Rating * 1000));
        }

        int n = 1;
        foreach (var p in puzzles)
        {
            Console.WriteLine($"#{n++} {p.Sudoku} - {Math.Round(p.Rating, 2)} - {p.HardestStrategy}");
        }
    }

    private static ArgumentsReader SetUpArgsReader()
    {
        var reader = new ArgumentsReader();
        
        reader.AddAllowedArgument("c", ArgumentValueType.Mandatory); //Count
        reader.AddAllowedArgument("s", ArgumentValueType.None); //Sort in descending order

        return reader;
    }
}