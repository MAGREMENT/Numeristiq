using Model.Core.Settings;
using Model.Core.Settings.Types;
using Model.Core.Trackers;
using Model.Sudokus.Solver;

namespace Model.Sudokus.Generator.Criterias;

public class MinimumRatingCriteria : EvaluationCriteria
{
    public const string OfficialName = "Minimum Rating";
    
    public MinimumRatingCriteria() : base(OfficialName, 
        new DoubleSetting("Rating",
            new SliderInteractionInterface(1, 3, 0.05)))
    {
    }

    public override bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker<SudokuStrategy, ISudokuSolveResult> usedStrategiesTracker)
    {
        return puzzle.Rating >= _settings[0].Get().ToDouble();
    }

    public override bool Equals(object? obj)
    {
        return obj is MinimumRatingCriteria;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}