using Model.Core;
using Model.Core.Generators;
using Model.Core.Trackers;

namespace ConsoleApplication.Commands.Abstracts;

public abstract class GenerateBatchCommand<TPuzzle, TState> : Command where TState : class
{
    private const int CountIndex = 0;
    private const int EvaluateIndex = 1;
    private const int SortIndex = 2;
    private const int SymmetryIndex = 3;
    private const int NotUniqueIndex = 4;

    private readonly IPuzzleGenerator<TPuzzle> _generator;
    
    public override string Description { get; }

    protected GenerateBatchCommand(string name, IPuzzleGenerator<TPuzzle> generator, params Option[] additionalOptions) 
        : base("GenerateBatch", additionalOptions.MergeWithReverseOrder(
            new Option("-c", "Count", ValueRequirement.Mandatory, ValueType.Int),
            new Option("-e", "Evaluates puzzles"),
            new Option("-s", "Sorts puzzles"),
            new Option("--symmetric", "Makes the puzzle symmetric around its center point"),
            new Option("--not-unique", "Allows the puzzle to not necessarily be unique")))
    {
        Description = $"Generates a determined amount of {name}'s";
        _generator = generator;
    }

    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var count = report.IsOptionUsed(CountIndex) ? (int)report.GetOptionValue(CountIndex)! : 1;
        SetUpGenerator(_generator, report);
        _generator.KeepSymmetry = report.IsOptionUsed(SymmetryIndex);
        _generator.KeepUniqueness = !report.IsOptionUsed(NotUniqueIndex);
        
        Console.WriteLine("Started generating...");
        var start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        var generated = _generator.Generate(count);
        Console.WriteLine($"Finished generating in {Math.Round((double)(DateTimeOffset.Now.ToUnixTimeMilliseconds() - start) / 1000, 4)}s");

        List<GeneratedPuzzle<TPuzzle>> result = new(count);

        if (report.IsOptionUsed(EvaluateIndex))
        {
            var solver = GetSolver(interpreter);
            var ratings = new RatingTracker();
            var hardest = new HardestStrategyTracker();

            ratings.AttachTo(solver);
            hardest.AttachTo(solver);
            
            Console.WriteLine("Started evaluating...");
            start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            foreach (var p in generated)
            {
                SetPuzzle(solver, p);
                solver.Solve();

                var puzzle = CreateGeneratedPuzzle(p);
                puzzle.SetEvaluation(ratings.Rating, hardest.Hardest!);
                result.Add(puzzle);
            }
            Console.WriteLine($"Finished evaluating in {Math.Round((double)(DateTimeOffset.Now.ToUnixTimeMilliseconds() - start) / 1000, 4)}s");
            
            ratings.Detach();
            hardest.Detach();
        }
        else
        {
            foreach (var p in generated)
            {
                result.Add(CreateGeneratedPuzzle(p));
            }
        }

        if (report.IsOptionUsed(SortIndex))
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
        
        foreach (var s in result)
        {
            Console.Write(s.AsString());
            if (s.Evaluated)
            {
                Console.Write($" # {Math.Round(s.Rating, 2)}");
                Console.Write($" - {s.Hardest!.Name}");
            }
            Console.WriteLine();
        }
    }

    protected virtual void SetUpGenerator(IPuzzleGenerator<TPuzzle> generator, IReadOnlyCallReport report)
    {
        
    }
    protected abstract ITrackerAttachableSolver<TState> GetSolver(ArgumentInterpreter interpreter);
    protected abstract GeneratedPuzzle<TPuzzle> CreateGeneratedPuzzle(TPuzzle puzzle);
    protected abstract void SetPuzzle(ISolver solver, TPuzzle puzzle);
}

public static class ArrayExtensions
{
    public static T[] MergeWithReverseOrder<T>(this T[] array, params T[] other)
    {
        var result = new T[array.Length + other.Length];
        Array.Copy(other, result, other.Length);
        Array.Copy(array, 0, result, other.Length, array.Length);
        return result;
    }
}