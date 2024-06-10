using Model.Core.Generators;
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
    
    public override string Description => "Generate a determined amount of Sudoku's";

    private readonly RandomEmptyTectonicGenerator _emptyGenerator = new();
    private readonly IPuzzleGenerator<ITectonic> _generator;
    
    public TectonicGenerateBatchCommand() : base("GenerateBatch",
        new Option("-c", "Count", ValueRequirement.Mandatory, ValueType.Int),
        new Option("--rc", "Row count, has priority over min and max value", ValueRequirement.Mandatory, ValueType.Int),
        new Option("--cc", "Column count, has priority over min and max value", ValueRequirement.Mandatory, ValueType.Int),
        new Option("--min-rc", "Minimum row count", ValueRequirement.Mandatory, ValueType.Int),
        new Option("--max-rc", "Maximum row count", ValueRequirement.Mandatory, ValueType.Int),
        new Option("--min-cc", "Minimum column count", ValueRequirement.Mandatory, ValueType.Int),
        new Option("--max-cc", "Maximum column count", ValueRequirement.Mandatory, ValueType.Int))
    {
        _generator = new RDRTectonicPuzzleGenerator(new BackTrackingFilledTectonicGenerator(_emptyGenerator));
    }
    
    public override void Execute(ArgumentInterpreter interpreter, IReadOnlyCallReport report)
    {
        var count = (int)(report.GetOptionValue(CountIndex) ?? 1);

        if (report.IsOptionUsed(RowCountIndex))
        {
            var value = (int)report.GetOptionValue(RowCountIndex)!;
            _emptyGenerator.MinRowCount = value;
            _emptyGenerator.MaxRowCount = value;
        }
        else
        {
            var value = report.GetOptionValue(MinRowCountIndex);
            if (value is not null) _emptyGenerator.MinRowCount = (int)value;
            value = report.GetOptionValue(MaxRowCountIndex);
            if (value is not null) _emptyGenerator.MaxRowCount = (int)value;
        }
        
        if (report.IsOptionUsed(ColumnCountIndex))
        {
            var value = (int)report.GetOptionValue(ColumnCountIndex)!;
            _emptyGenerator.MinColumnCount = value;
            _emptyGenerator.MaxColumnCount = value;
        }
        else
        {
            var value = report.GetOptionValue(MinColumnCountIndex);
            if (value is not null) _emptyGenerator.MinColumnCount = (int)value;
            value = report.GetOptionValue(MaxColumnCountIndex);
            if (value is not null) _emptyGenerator.MaxColumnCount = (int)value;
        }
        
        Console.WriteLine("Started generating...");
        var start = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        for (int i = 0; i < count; i++)
        {
            var generated = _generator.Generate();
            Console.WriteLine(TectonicTranslator.TranslateRdFormat(generated));
        }
        
        Console.WriteLine($"Finished generating in {Math.Round((double)(DateTimeOffset.Now.ToUnixTimeMilliseconds() - start) / 1000, 4)}s");
    }
}