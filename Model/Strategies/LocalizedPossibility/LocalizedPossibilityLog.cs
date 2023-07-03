namespace Model.Strategies.LocalizedPossibility;

public class LocalizedPossibilityLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level => StrategyLevel.Medium;

    public LocalizedPossibilityLog(int number, int row, int col)
    {
        AsString = $"{number} removed from the possibilities in row {row}, column {col} because of localized possibility";
    }

}