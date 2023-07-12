namespace Model.Strategies.SinglePossibility;

public class SinglePossibilityLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level => StrategyLevel.Basic;

    public SinglePossibilityLog(int number, int row, int col)
    {
        AsString = $"[{row + 1}, {col + 1}] {number} added as definitive because of single possibility";
    }
}