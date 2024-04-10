using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Trackers;

namespace Model.Sudoku.Generator.Criterias;

public class HardestDifficultyCriteria : IEvaluationCriteria
{
    public StrategyDifficulty MinimumDifficulty { get; set; }
    
    public StrategyDifficulty MaximumDifficulty { get; set; }

    public bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker t) =>
        puzzle.Hardest is not null && puzzle.Hardest.Difficulty >= MinimumDifficulty
        && puzzle.Hardest.Difficulty <= MaximumDifficulty;
}