using Model.Core;
using Model.Core.Trackers;
using Model.Helpers.Settings.Types;
using Model.Sudokus.Solver;
using Model.Utility;

namespace Model.Sudokus.Generator.Criterias;

public class MinimumHardestDifficultyCriteria : EvaluationCriteria
{
    public const string OfficialName = "Minimum Hardest Strategy";
    
    public MinimumHardestDifficultyCriteria() : base(OfficialName, 
        new EnumSetting<StepDifficulty>("Difficulty",
            new SpaceConverter(), StepDifficulty.Basic))
    {
    }

    public override bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker<SudokuStrategy, ISudokuSolveResult> usedStrategiesTracker)
    {
        return puzzle.Hardest is not null &&
               puzzle.Hardest.Difficulty >= ((EnumSetting<StepDifficulty>)_settings[0]).Value;
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