using Model.Core;
using Model.Core.Generators;
using Model.Core.Trackers;

namespace ConsoleApplication.Commands.Abstracts;

public abstract class GenerateBatchCommand<T> : Command
{
    private const int CountIndex = 0;
    private const int EvaluateIndex = 1;
    private const int SortIndex = 2;

    private readonly IPuzzleGenerator<T> _generator;
    
    public override string Description { get; }

    protected GenerateBatchCommand(string name, IPuzzleGenerator<T> generator, params Option[] additionalOptions) 
        : base("GenerateBatch", additionalOptions.MergeWithReverseOrder(
            new Option("-c", "Count", ValueRequirement.Mandatory, ValueType.Int),
            new Option("-e", "Evaluates puzzles"),
            new Option("-s", "Sorts puzzles")))
    {
        Description = $"Generates a determined amount of {name}'s";
        _generator = generator;
    }

    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var count = report.IsOptionUsed(CountIndex) ? (int)report.GetOptionValue(CountIndex)! : 1;
        SetUpGenerator(_generator, report);
        
        Console.WriteLine("Started generating...");
        var start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        var generated = _generator.Generate(count);
        Console.WriteLine($"Finished generating in {Math.Round((double)(DateTimeOffset.Now.ToUnixTimeMilliseconds() - start) / 1000, 4)}s");

        List<GeneratedPuzzle<T>> result = new(count);

        if (report.IsOptionUsed(EvaluateIndex))
        {
            var (solver, ratings, hardest) = GetSolverWithAttachedTracker(interpreter);
            
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

    protected virtual void SetUpGenerator(IPuzzleGenerator<T> generator, IReadOnlyCallReport report)
    {
        
    }
    protected abstract (ISolver, IRatingTracker, IHardestStrategyTracker) GetSolverWithAttachedTracker(ArgumentInterpreter interpreter);
    protected abstract GeneratedPuzzle<T> CreateGeneratedPuzzle(T puzzle);
    protected abstract void SetPuzzle(ISolver solver, T puzzle);
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