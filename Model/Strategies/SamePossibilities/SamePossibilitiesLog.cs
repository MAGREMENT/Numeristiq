namespace Model.Strategies.SamePossibilities;

public class SamePossibilitiesLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level => StrategyLevel.Medium;

    public SamePossibilitiesLog(int number, int row, int col)
    {
        AsString = $"{number} removed from the possibilities in row {row}, column {col} because of same possibilities";
    }

}