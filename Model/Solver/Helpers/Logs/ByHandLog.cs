using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;

namespace Model.Solver.Helpers.Logs;

public class ByHandLog : ISolverLog
{
    public int Id { get; }
    public string Title => "Removed by hand";
    public Intensity Intensity => Intensity.Six;
    public string Changes { get; }
    public string Explanation => "This possibility was removed by hand";
    public SolverState StateBefore { get; }
    public SolverState StateAfter { get; }
    public HighlightManager HighlightManager => new(new DelegateHighlighter(HighLight));

    private readonly SolverChange _change;

    public ByHandLog(int id, int possibility, int row, int col, SolverState stateBefore, SolverState stateAfter)
    {
        Id = id;
        Changes = $"[{row + 1}, {col + 1}] {possibility} removed by hand";
        StateBefore = stateBefore;
        StateAfter = stateAfter;
        _change = new SolverChange(ChangeType.Possibility, possibility, row, col);
    }

    private void HighLight(IHighlightable highlightable)
    {
        highlightable.HighlightPossibility(_change.Number, _change.Row, _change.Column, ChangeColoration.ChangeOne);
    }
}