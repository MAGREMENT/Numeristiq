using Model.Core;
using Model.Core.Settings.Types;
using Model.Core.Trackers;
using Model.Utility;

namespace Model.Sudokus.Generator.Criterias;

public class MaximumHardestDifficultyCriteria : EvaluationCriteria
{
    public const string OfficialName = "Maximum Hardest Strategy";
    
    public MaximumHardestDifficultyCriteria() : base(OfficialName, 
        new EnumSetting<StepDifficulty>("Difficulty", "The evaluator cannot use a strategy with a higher difficulty",
            new SpaceConverter(), StepDifficulty.Extreme))
    {
    }

    public override bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker usedStrategiesTracker)
    {
        return puzzle.Hardest is not null &&
               puzzle.Hardest.Difficulty <= ((EnumSetting<StepDifficulty>)_settings[0]).Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is MaximumHardestDifficultyCriteria;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}