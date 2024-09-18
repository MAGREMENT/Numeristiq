using Model.Core;
using Model.Core.Settings.Types;
using Model.Core.Trackers;
using Model.Sudokus.Solver;
using Model.Utility;

namespace Model.Sudokus.Generator.Criterias;

public class MinimumHardestDifficultyCriteria : EvaluationCriteria
{
    public const string OfficialName = "Minimum Hardest Strategy";
    
    public MinimumHardestDifficultyCriteria() : base(OfficialName, 
        new EnumSetting<Difficulty>("Difficulty", "The evaluator must use at least one strategy with a higher difficulty",
            new CamelCaseToSpacedConverter(), Difficulty.Basic))
    {
    }

    public override bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker usedStrategiesTracker)
    {
        return puzzle.Hardest is not null &&
               puzzle.Hardest.Difficulty >= ((EnumSetting<Difficulty>)_settings[0]).Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is MinimumHardestDifficultyCriteria;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}