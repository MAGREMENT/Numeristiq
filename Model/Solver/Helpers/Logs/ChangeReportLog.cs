using Model.Solver.Helpers.Changes;

namespace Model.Solver.Helpers.Logs;

public class ChangeReportLog : ISolverLog
{
    public int Id { get; }
    public string Title { get; }
    public Intensity Intensity { get; }
    public string Changes { get; }
    public string Explanation { get; }
    public string SolverState { get; }
    public HighlightManager HighlightManager  { get; }

    public ChangeReportLog(int id, IStrategy strategy, ChangeReport report, string solverState)
    {
        Id = id;
        Title = strategy.Name;
        Intensity = (Intensity)strategy.Difficulty;
        Changes = report.Changes;
        Explanation = report.Explanation;
        SolverState = solverState;
        HighlightManager = report.HighlightManager;
    }
}