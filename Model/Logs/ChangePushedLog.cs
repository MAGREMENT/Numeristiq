using System.Collections.Generic;

namespace Model.Logs;

public class ChangePushedLog : ISolverLog
{
    public int Id { get; }
    public string Title { get; }
    public Intensity Intensity { get; }

    public string Changes
    {
        get
        {
            //TODO
            return "";
        }
    }

    public string Explanation { get; }

    public string SolverState { get; }
    public HighLightCause CauseHighLighter { get; }

    public ChangePushedLog(int id, IStrategy strategy, IChangeReport report, string solverState)
    {
        Id = id;
        Title = strategy.Name;
        Intensity = (Intensity)strategy.Difficulty;
        Explanation = report.Explanation;
        SolverState = solverState;
        CauseHighLighter = report.CauseHighLighter;
    }
}