using Model.Sudoku.Solver.Trackers;

namespace Model.Sudoku.Generator.Criterias;

public class MustUseStrategyCriteria : IEvaluationCriteria
{
    public string StrategyName { get; set; } = string.Empty;

    public string Name => "Must Use Strategy";

    public bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker usedStrategiesTracker) =>
        usedStrategiesTracker.WasUsed(StrategyName);
    
    public override bool Equals(object? obj)
    {
        return obj is MustUseStrategyCriteria criteria && criteria.StrategyName.Equals(StrategyName);
    }

    public override int GetHashCode()
    {
        return StrategyName.GetHashCode();
    }
}