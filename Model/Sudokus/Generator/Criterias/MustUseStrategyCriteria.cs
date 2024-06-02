using System;
using System.Collections.Generic;
using Model.Core.Trackers;
using Model.Helpers.Settings;
using Model.Helpers.Settings.Types;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Strategies;

namespace Model.Sudokus.Generator.Criterias;

public class MustUseStrategyCriteria : EvaluationCriteria
{
    public const string OfficialName = "Must Use Strategy";
    
    public MustUseStrategyCriteria(IReadOnlyList<string> usedStrategies) : base(OfficialName, 
        new StringSetting("StrategyName", new AutoFillingInteractionInterface(usedStrategies),
            NakedSingleStrategy.OfficialName))
    {
    }

    public override bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker<SudokuStrategy, ISudokuSolveResult> usedStrategiesTracker)
    {
        return usedStrategiesTracker.WasUsed(_settings[0].Get().ToString()!);
    }

    public override bool Equals(object? obj)
    {
        return obj is MustUseStrategyCriteria criteria &&
               criteria.Settings[0].Get().Equals(Settings[0].Get());
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, _settings[0].Get().GetHashCode());
    }
}