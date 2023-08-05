using Model.Logs;

namespace Model;

public interface IChangeReport
{
    public string Explanation { get; }

    public HighLightCause CauseHighLighter { get; }

    public static void DefaultCauseHighLighter(IHighLighter highLighter) { }

    public void Process();
}

public interface IHighLighter
{
    public void HighLightPossibility(int possibility, int row, int col, ChangeColoration coloration);

    public void HighLightCell(int row, int col, ChangeColoration coloration);
}

public enum ChangeColoration
{
    Change, CauseOne, CauseTwo
}

public delegate void HighLightCause(IHighLighter h);