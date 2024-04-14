using Model.Sudoku.Solver.Trackers;

namespace Model.Sudoku.Generator.Criterias;

public class MinimumRatingCriteria : IEvaluationCriteria
{
    public double Rating { get; set; }

    public string Name => "Minimum Rating";
    
    public bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker t) => puzzle.Rating >= Rating;

    public override bool Equals(object? obj)
    {
        return obj is MinimumRatingCriteria;
    }

    public override int GetHashCode()
    {
        return typeof(MinimumRatingCriteria).GetHashCode();
    }
}