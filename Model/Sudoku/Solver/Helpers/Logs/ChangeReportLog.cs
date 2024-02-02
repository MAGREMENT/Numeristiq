using Model.Sudoku.Solver.Explanation;
using Model.Sudoku.Solver.Helpers.Changes;
using Model.Sudoku.Solver.Helpers.Highlighting;

namespace Model.Sudoku.Solver.Helpers.Logs;

public class ChangeReportLog : ISolverLog
{
    public int Id { get; }
    public string Title { get; }
    public Intensity Intensity { get; }
    public string Changes { get; }
    public string Description { get; }
    public ExplanationElement? Explanation { get; }
    public SolverState StateBefore { get; }
    public SolverState StateAfter { get; }
    public HighlightManager HighlightManager  { get; }
    public bool FromSolving => true;


    public ChangeReportLog(int id, IStrategy strategy, ChangeReport report, SolverState stateBefore,
        SolverState stateAfter)
    {
        Id = id;
        Title = strategy.Name;
        Intensity = (Intensity)strategy.Difficulty;
        Changes = report.Changes;
        Description = report.Description;
        StateBefore = stateBefore;
        StateAfter = stateAfter;
        HighlightManager = report.HighlightManager;
        Explanation = report.Explanation;
    }
}