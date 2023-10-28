using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;

namespace Model.Solver.Helpers.Logs;

public class ChangeReportLog : ISolverLog
{
    public int Id { get; }
    public string Title { get; }
    public Intensity Intensity { get; }
    public string Changes { get; }
    public string Explanation { get; }
    public SolverState StateBefore { get; }
    public SolverState StateAfter { get; }
    public HighlightManager HighlightManager  { get; }

    public ChangeReportLog(int id, IStrategy strategy, ChangeReport report, SolverState stateBefore, SolverState stateAfter)
    {
        Id = id;
        Title = strategy.Name;
        Intensity = (Intensity)strategy.Difficulty;
        Changes = report.Changes;
        Explanation = report.Explanation;
        StateBefore = stateBefore;
        StateAfter = stateAfter;
        HighlightManager = report.HighlightManager;
    }
}