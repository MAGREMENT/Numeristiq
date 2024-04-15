using System;
using Model.Helpers.Settings.Types;
using Model.Sudoku.Solver.Strategies;
using Model.Sudoku.Solver.Trackers;

namespace Model.Sudoku.Generator.Criterias;

public class MustUseStrategyCriteria : EvaluationCriteria
{
    public const string OfficialName = "Must Use Strategy";
    
    public MustUseStrategyCriteria() : base(OfficialName, 
        new StringSetting("StrategyName", null! /*TODO*/, NakedSingleStrategy.OfficialName))
    {
    }

    public override bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker usedStrategiesTracker)
    {
        return usedStrategiesTracker.WasUsed(_settings[0].ToString()!);
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