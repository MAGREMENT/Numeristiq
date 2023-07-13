namespace Model.DeprecatedStrategies.SamePossibilities;

public class SamePossibilitiesLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level => StrategyLevel.Medium;

    public SamePossibilitiesLog(int number, int row, int col)
    {
        AsString = $"[{row + 1}, {col + 1}] {number} removed from possibilities because of same possibilities";
    }

}