using System;
using System.Collections.Generic;
using Model.Core.Settings;
using Model.Core.Settings.Types;
using Model.Core.Trackers;
using Model.Sudokus.Solver.Strategies;

namespace Model.Sudokus.Generator.Criterias;

public class CantUseStrategyCriteria : EvaluationCriteria
{
    public const string OfficialName = "Can't Use Strategy";
    
    public CantUseStrategyCriteria(IReadOnlyList<string> strategies) : base(OfficialName, 
        new StringSetting("StrategyName", "A strategy that cannot be used by the evaluator",
            new AutoFillingInteractionInterface(strategies), NakedSingleStrategy.OfficialName))
    {
    }

    public override bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker usedStrategiesTracker)
    {
        return !usedStrategiesTracker.WasUsed(_settings[0].Get().ToString()!);
    }

    public override bool Equals(object? obj)
    {
        return obj is CantUseStrategyCriteria criteria &&
               criteria.Settings[0].Get().Equals(Settings[0].Get());
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, _settings[0].Get().GetHashCode());
    }
}