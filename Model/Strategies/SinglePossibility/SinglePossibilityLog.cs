namespace Model.Strategies.SinglePossibility;

public class SinglePossibilityLog : ISolverLog
{
    private readonly string _string;

    public SinglePossibilityLog(int number, int row, int col)
    {
        _string = $"{number} added in row {row}, column {col} because of single possibility";
    }

    public string ViewLog()
    {
        return _string;
    }

}