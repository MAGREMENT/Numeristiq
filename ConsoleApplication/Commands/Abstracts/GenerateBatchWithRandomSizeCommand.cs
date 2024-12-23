﻿using Model.Core.Generators;
using Model.Utility;

namespace ConsoleApplication.Commands.Abstracts;

public abstract class GenerateBatchWithRandomSizeCommand<TPuzzle, TState> : GenerateBatchCommand<TPuzzle, TState> where TState : class
{
    private const int RowCountIndex = 6;
    private const int ColumnCountIndex = 7;
    private const int MinRowCountIndex = 8;
    private const int MaxRowCountIndex = 9;
    private const int MinColumnCountIndex = 10;
    private const int MaxColumnCountIndex = 11;
    
    protected GenerateBatchWithRandomSizeCommand(string name, IPuzzleGenerator<TPuzzle> generator) : base(name, generator, 
        new Option("--rc", "Row count, has priority over min and max value", ValueRequirement.Mandatory, ValueType.Int),
        new Option("--cc", "Column count, has priority over min and max value", ValueRequirement.Mandatory, ValueType.Int),
        new Option("--min-rc", "Minimum row count", ValueRequirement.Mandatory, ValueType.Int),
        new Option("--max-rc", "Maximum row count", ValueRequirement.Mandatory, ValueType.Int),
        new Option("--min-cc", "Minimum column count", ValueRequirement.Mandatory, ValueType.Int),
        new Option("--max-cc", "Maximum column count", ValueRequirement.Mandatory, ValueType.Int))
    {
    }

    protected override bool SetUpGenerator(IPuzzleGenerator<TPuzzle> generator, IReadOnlyCallReport report)
    {
        if (!CheckSizeOptionsValues(GetOptionsValues(report))) return false;
        var randomizer = GetRandomizer(generator);
        
        if (report.IsOptionUsed(RowCountIndex))
        {
            var value = (int)report.GetOptionValue(RowCountIndex)!;
            randomizer.MinRowCount = value;
            randomizer.MaxRowCount = value;
        }
        else
        {
            var value = report.GetOptionValue(MinRowCountIndex);
            if (value is not null) randomizer.MinRowCount = (int)value;
            value = report.GetOptionValue(MaxRowCountIndex);
            if (value is not null) randomizer.MaxRowCount = (int)value;
        }
        
        if (report.IsOptionUsed(ColumnCountIndex))
        {
            var value = (int)report.GetOptionValue(ColumnCountIndex)!;
            randomizer.MinColumnCount = value;
            randomizer.MaxColumnCount = value;
        }
        else
        {
            var value = report.GetOptionValue(MinColumnCountIndex);
            if (value is not null) randomizer.MinColumnCount = (int)value;
            value = report.GetOptionValue(MaxColumnCountIndex);
            if (value is not null) randomizer.MaxColumnCount = (int)value;
        }

        return true;
    }

    protected abstract GridSizeRandomizer GetRandomizer(IPuzzleGenerator<TPuzzle> generator);

    protected virtual bool CheckSizeOptionsValues(IEnumerable<(Option, int)> options)
    {
        return true;
    }

    private IEnumerable<(Option, int)> GetOptionsValues(IReadOnlyCallReport report)
    {
        for (int i = RowCountIndex; i <= MaxColumnCountIndex; i++)
        {
            if (report.IsOptionUsed(i))
            {
                yield return (Options[i], (int)report.GetOptionValue(i)!);
            }
        }
    }
}