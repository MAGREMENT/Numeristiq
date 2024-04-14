using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Trackers;

namespace Model.Sudoku.Generator.Criterias;

public class MaximumHardestDifficultyCriteria : IEvaluationCriteria
{
    public StrategyDifficulty Difficulty { get; set; }

    public string Name => "Maximum Hardest Difficulty";

    public bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker t) =>
        puzzle.Hardest is not null && puzzle.Hardest.Difficulty <= Difficulty;
    
    public override bool Equals(object? obj)
    {
        return obj is MaximumHardestDifficultyCriteria;
    }

    public override int GetHashCode()
    {
        return typeof(MaximumHardestDifficultyCriteria).GetHashCode();
    }
}