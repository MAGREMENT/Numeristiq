using Model.Helpers.Settings.Types;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Trackers;
using Model.Utility;

namespace Model.Sudoku.Generator.Criterias;

public class MaximumHardestDifficultyCriteria : EvaluationCriteria
{
    public const string OfficialName = "Maximum Hardest Strategy";
    
    public MaximumHardestDifficultyCriteria() : base(OfficialName, 
        new EnumSetting<StrategyDifficulty>("Difficulty",
            new SpaceConverter(), StrategyDifficulty.Extreme))
    {
    }

    public override bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker usedStrategiesTracker)
    {
        return puzzle.Hardest is not null &&
               puzzle.Hardest.Difficulty <= ((EnumSetting<StrategyDifficulty>)_settings[0]).Value;
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