namespace Model.Strategies.SinglePossibility;

public class SinglePossibilityLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level => StrategyLevel.Easy;

    public SinglePossibilityLog(int number, int row, int col)
    {
        AsString = $"{number} added in row {row}, column {col} because of single possibility";
    }
}