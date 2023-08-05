namespace Model.Logs;

public class ByHandRemovedLog : ISolverLog
{
    public int Id { get; }
    public string Title => "Removed by hand";
    public Intensity Intensity => Intensity.Six;
    public string Changes { get; }
    public string Explanation => "This possibility was removed by hand";
    public string SolverState { get; }
    public HighLightCause CauseHighLighter => HighLight;

    private readonly LogChange _asChange;

    public ByHandRemovedLog(int id, int possibility, int row, int col, string solverState)
    {
        Id = id;
        Changes = $"[{row + 1}, {col + 1}] {possibility} removed by hand";
        SolverState = solverState;
        _asChange = new LogChange(SolverNumberType.Possibility, possibility, row, col);
    }

    private void HighLight(IHighLighter highLighter)
    {
        highLighter.HighLightPossibility(_asChange.Number, _asChange.Row, _asChange.Column, ChangeColoration.Change);
    }
}