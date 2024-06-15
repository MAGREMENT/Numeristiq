using Model.Core.Generators;
using Model.Core.Trackers;
using Model.Tectonics;
using Model.Tectonics.Generator;

namespace ConsoleApplication.Commands;

public class TectonicGenerateBatchCommand : Command
{
    private const int CountIndex = 0;
    private const int RowCountIndex = 1;
    private const int ColumnCountIndex = 2;
    private const int MinRowCountIndex = 3;
    private const int MaxRowCountIndex = 4;
    private const int MinColumnCountIndex = 5;
    private const int MaxColumnCountIndex = 6;
    private const int EvaluateIndex = 7;
    private const int SortIndex = 8;
    
    public override string Description => "Generate a determined amount of Sudoku's";

    private readonly RandomLayoutBackTrackingFilledTectonicGenerator _filledGenerator = new();
    private readonly IPuzzleGenerator<ITectonic> _generator;
    
    public TectonicGenerateBatchCommand() : base("GenerateBatch",
        new Option("-c", "Count", ValueRequirement.Mandatory, ValueType.Int),
        new Option("--rc", "Row count, has priority over min and max value", ValueRequirement.Mandatory, ValueType.Int),
        new Option("--cc", "Column count, has priority over min and max value", ValueRequirement.Mandatory, ValueType.Int),
        new Option("--min-rc", "Minimum row count", ValueRequirement.Mandatory, ValueType.Int),
        new Option("--max-rc", "Maximum row count", ValueRequirement.Mandatory, ValueType.Int),
        new Option("--min-cc", "Minimum column count", ValueRequirement.Mandatory, ValueType.Int),
        new Option("--max-cc", "Maximum column count", ValueRequirement.Mandatory, ValueType.Int),
        new Option("-e", "Evaluates puzzles"),
        new Option("-s", "Sorts puzzles"))
    {
        _generator = new RDRTectonicPuzzleGenerator(_filledGenerator);
    }
    
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var count = (int)(report.GetOptionValue(CountIndex) ?? 1);

        if (report.IsOptionUsed(RowCountIndex))
        {
            var value = (int)report.GetOptionValue(RowCountIndex)!;
            _filledGenerator.MinRowCount = value;
            _filledGenerator.MaxRowCount = value;
        }
        else
        {
            var value = report.GetOptionValue(MinRowCountIndex);
            if (value is not null) _filledGenerator.MinRowCount = (int)value;
            value = report.GetOptionValue(MaxRowCountIndex);
            if (value is not null) _filledGenerator.MaxRowCount = (int)value;
        }
        
        if (report.IsOptionUsed(ColumnCountIndex))
        {
            var value = (int)report.GetOptionValue(ColumnCountIndex)!;
            _filledGenerator.MinColumnCount = value;
            _filledGenerator.MaxColumnCount = value;
        }
        else
        {
            var value = report.GetOptionValue(MinColumnCountIndex);
            if (value is not null) _filledGenerator.MinColumnCount = (int)value;
            value = report.GetOptionValue(MaxColumnCountIndex);
            if (value is not null) _filledGenerator.MaxColumnCount = (int)value;
        }
        
        Console.WriteLine("Started generating...");
        var start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        var generated = _generator.Generate(count);
        Console.WriteLine($"Finished generating in {Math.Round((double)(DateTimeOffset.Now.ToUnixTimeMilliseconds() - start) / 1000, 4)}s");

        List<GeneratedTectonicPuzzle> result = new(count);

        if (report.IsOptionUsed(EvaluateIndex))
        {
            var solver = interpreter.Instantiator.InstantiateTectonicSolver();

            var ratings = new RatingTracker<TectonicStrategy, object>();
            var hardest = new HardestStrategyTracker<TectonicStrategy, object>();

            ratings.AttachTo(solver);
            hardest.AttachTo(solver);
            
            Console.WriteLine("Started evaluating...");
            start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            foreach (var t in generated)
            {
                solver.SetTectonic(t.Copy());
                solver.Solve();

                var puzzle = new GeneratedTectonicPuzzle(t);
                puzzle.SetEvaluation(ratings.Rating, hardest.Hardest!);
                result.Add(puzzle);
            }
            Console.WriteLine($"Finished evaluating in {Math.Round((double)(DateTimeOffset.Now.ToUnixTimeMilliseconds() - start) / 1000, 4)}s");
            
            ratings.Detach();
            hardest.Detach();
        }
        else
        {
            foreach (var t in generated)
            {
                result.Add(new GeneratedTectonicPuzzle(t));
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
        
        var n = 1;
        foreach (var s in result)
        {
            Console.Write($"#{n++} {s.AsString()}");
            if (s.Evaluated)
            {
                Console.Write($" - {Math.Round(s.Rating, 2)}");
                Console.Write($" - {s.Hardest!.Name}");
            }
            Console.WriteLine();
        }
    }
}