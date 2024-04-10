using Model.Sudoku.Solver.Trackers;

namespace Model.Sudoku.Generator.Criterias;

public class RatingCriteria : IEvaluationCriteria
{
    public int Min { get; set; }
    
    public int Max { get; set; }

    public bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker t) => puzzle.Rating >= Min && puzzle.Rating <= Max;
}