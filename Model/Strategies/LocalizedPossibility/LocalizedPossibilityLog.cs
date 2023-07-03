namespace Model.Strategies.LocalizedPossibility;

public class LocalizedPossibilityLog : ISolverLog
{
    private readonly string _string;

    public LocalizedPossibilityLog(int number, int row, int col)
    {
        _string = $"{number} removed from the possibilities in row {row}, column {col} because of localized possibility";
    }

    public string ViewLog()
    {
        return _string;
    }
}