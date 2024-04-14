using Model.Sudoku.Solver.Trackers;

namespace Model.Sudoku.Generator.Criterias;

public class CantUseStrategyCriteria : IEvaluationCriteria
{
    public string StrategyName { get; set; } = string.Empty;

    public string Name => "Can't Use Strategy";

    public bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker usedStrategiesTracker) =>
        !usedStrategiesTracker.WasUsed(StrategyName);

    public override bool Equals(object? obj)
    {
        return obj is CantUseStrategyCriteria criteria && criteria.StrategyName.Equals(StrategyName);
    }

    public override int GetHashCode()
    {
        return StrategyName.GetHashCode();
    }
}