using Model.Sudoku.Solver.Trackers;

namespace Model.Sudoku.Generator.Criterias;

public class MaximumRatingCriteria : IEvaluationCriteria
{
    public double Rating { get; set; }

    public string Name => "Maximum Rating";
    public bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker t) => puzzle.Rating >= Rating;
    
    public override bool Equals(object? obj)
    {
        return obj is MaximumRatingCriteria;
    }

    public override int GetHashCode()
    {
        return typeof(MaximumRatingCriteria).GetHashCode();
    }
}