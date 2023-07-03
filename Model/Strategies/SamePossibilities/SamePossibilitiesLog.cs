namespace Model.Strategies.SamePossibilities;

public class SamePossibilitiesLog : ISolverLog
{
    private readonly string _string;

    public SamePossibilitiesLog(int number, int row, int col)
    {
        _string = $"{number} removed from the possibilities in row {row}, column {col} because of same possibilities";
    }

    public string ViewLog()
    {
        return _string;
    }

}