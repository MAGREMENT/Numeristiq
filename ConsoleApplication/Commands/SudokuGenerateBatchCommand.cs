using Model.Sudoku.Generator;
using Model.Sudoku.Solver.Trackers;

namespace ConsoleApplication.Commands;

public class SudokuGenerateBatchCommand : Command
{
    private const int CountIndex = 0;
    private const int EvaluateIndex = 1;
    private const int SortIndex = 2;
    
    public override string Description => "Generate a determined amount of Sudoku's";
    
    private readonly ISudokuPuzzleGenerator _generator = new RCRSudokuPuzzleGenerator(new BackTrackingFilledSudokuGenerator());
    
    public SudokuGenerateBatchCommand() : base("SudokuGenerateBatch", 
        new Option("-c", "Count", OptionValueRequirement.Mandatory, OptionValueType.Int),
        new Option("-e", "Evaluate puzzles"),
        new Option("-s", "Sort Sudoku's"))
    {
    }
    
    public override void Execute(IReadOnlyArgumentInterpreter interpreter, IReadOnlyOptionsReport report)
    {
        var count = report.IsUsed(CountIndex) ? (int)report.GetValue(CountIndex)! : 1;
        
        Console.WriteLine("Started generating...");
        var start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        var generated = _generator.Generate(count);
        Console.WriteLine($"Finished generating in {Math.Round((double)(DateTimeOffset.Now.ToUnixTimeMilliseconds() - start) / 1000, 4)}s");

        List<GeneratedSudokuPuzzle> result = new(count);

        if (report.IsUsed(EvaluateIndex))
        {
            if (!interpreter.Instantiator.InstantiateSudokuSolver(out var solver)) return;

            var ratings = new RatingTracker();
            var hardest = new HardestStrategyTracker();

            solver.AddTracker(ratings);
            solver.AddTracker(hardest);
            
            Console.WriteLine("Started evaluating...");
            start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            foreach (var s in generated)
            {
                solver.SetSudoku(s.Copy());
                solver.Solve();

                var puzzle = new GeneratedSudokuPuzzle(s);
                puzzle.SetEvaluation(ratings.Rating, hardest.Hardest!);
                result.Add(puzzle);
            }
            Console.WriteLine($"Finished evaluating in {Math.Round((double)(DateTimeOffset.Now.ToUnixTimeMilliseconds() - start) / 1000, 4)}s");
            
            solver.RemoveTracker(ratings);
            solver.RemoveTracker(hardest);
        }
        else
        {
            foreach (var s in generated)
            {
                result.Add(new GeneratedSudokuPuzzle(s));
            }
        }

        if (report.IsUsed(SortIndex))
        {
            Console.WriteLine("Started sorting...");
            start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            result.Sort((s1, s2) =>
            {
                var r = (int)(s2.Rating * 1000 - s1.Rating * 1000);
                if (r != 0) return r;

                if (s1.Hardest is null || s2.Hardest is null) return 0;

                return (int)s2.Hardest.Difficulty - (int)s1.Hardest.Difficulty;
            });
            Console.WriteLine($"Finished sorting in {Math.Round((double)(DateTimeOffset.Now.ToUnixTimeMilliseconds() - start) / 1000, 4)}s");
        }
        
        var n = 1;
        foreach (var s in result)
        {
            Console.Write($"#{n++} {s.AsString()}");
            if (s.Evaluated)
            {
                Console.Write($" - {Math.Round(s.Rating, 2)}");
                Console.Write($" - {s.Hardest}");
            }
            Console.WriteLine();
        }
    }
}