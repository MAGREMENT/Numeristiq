using Model.Helpers.Settings.Types;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Trackers;
using Model.Utility;

namespace Model.Sudoku.Generator.Criterias;

public class MinimumHardestDifficultyCriteria : EvaluationCriteria
{
    public const string OfficialName = "Minimum Hardest Strategy";
    
    public MinimumHardestDifficultyCriteria() : base(OfficialName, 
        new EnumSetting<StrategyDifficulty>("Difficulty",
            new SpaceConverter(), StrategyDifficulty.Basic))
    {
    }

    public override bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker usedStrategiesTracker)
    {
        return puzzle.Hardest is not null &&
               puzzle.Hardest.Difficulty >= ((EnumSetting<StrategyDifficulty>)_settings[0]).Value;
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