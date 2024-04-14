using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Trackers;

namespace Model.Sudoku.Generator.Criterias;

public class MinimumHardestDifficultyCriteria : IEvaluationCriteria
{
    public StrategyDifficulty Difficulty { get; set; }

    public string Name => "Minimum Hardest Difficulty";

    public bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker t) =>
        puzzle.Hardest is not null && puzzle.Hardest.Difficulty >= Difficulty;
    
    public override bool Equals(object? obj)
    {
        return obj is MinimumHardestDifficultyCriteria;
    }

    public override int GetHashCode()
    {
        return typeof(MinimumHardestDifficultyCriteria).GetHashCode();
    }
}