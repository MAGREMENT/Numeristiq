namespace Model.Strategies.LocalizedPossibility;

public class LocalizedPossibilityLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level => StrategyLevel.Medium;

    public LocalizedPossibilityLog(int number, int row, int col)
    {
        AsString = $"[{row + 1}, {col + 1}] {number} removed from possibilities because of localized possibilities";
    }

}