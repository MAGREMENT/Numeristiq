using Model.Helpers.Settings;
using Model.Helpers.Settings.Types;
using Model.Sudoku.Solver.Trackers;

namespace Model.Sudoku.Generator.Criterias;

public class MaximumRatingCriteria : EvaluationCriteria
{
    public const string OfficialName = "Maximum Rating";
    
    public MaximumRatingCriteria() : base(OfficialName, 
        new DoubleSetting("Rating",
            new SliderInteractionInterface(1, 3, 0.05), 3))
    {
    }

    public override bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker usedStrategiesTracker)
    {
        return puzzle.Rating <= _settings[0].Get().ToDouble();
    }

    public override bool Equals(object? obj)
    {
        return obj is MaximumRatingCriteria;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}