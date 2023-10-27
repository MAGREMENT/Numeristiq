using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;

namespace Model.Solver.Helpers.Logs;

public class ByHandRemovedLog : ISolverLog
{
    public int Id { get; }
    public string Title => "Removed by hand";
    public Intensity Intensity => Intensity.Six;
    public string Changes { get; }
    public string Explanation => "This possibility was removed by hand";
    public SolverState SolverState { get; }
    public HighlightManager HighlightManager => new(new DelegateHighlighter(HighLight));

    private readonly SolverChange _asChange;

    public ByHandRemovedLog(int id, int possibility, int row, int col, SolverState solverState)
    {
        Id = id;
        Changes = $"[{row + 1}, {col + 1}] {possibility} removed by hand";
        SolverState = solverState;
        _asChange = new SolverChange(ChangeType.Possibility, possibility, row, col);
    }

    private void HighLight(IHighlightable highlightable)
    {
        highlightable.HighlightPossibility(_asChange.Number, _asChange.Row, _asChange.Column, ChangeColoration.ChangeOne);
    }
}