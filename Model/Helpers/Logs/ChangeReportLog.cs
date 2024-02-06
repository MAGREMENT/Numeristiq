using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Explanation;

namespace Model.Helpers.Logs;

public class ChangeReportLog : ISolverLog
{
    public int Id { get; }
    public string Title { get; }
    public Intensity Intensity { get; }
    public IReadOnlyList<SolverChange> Changes { get; }
    public string Description { get; }
    public ExplanationElement? Explanation { get; }
    public SolverState StateBefore { get; }
    public SolverState StateAfter { get; }
    public HighlightManager HighlightManager  { get; }
    public bool FromSolving => true;


    public ChangeReportLog(int id, ICommitMaker maker, IReadOnlyList<SolverChange> changes, ChangeReport report,
        SolverState stateBefore, SolverState stateAfter)
    {
        Id = id;
        Title = maker.Name;
        Intensity = (Intensity)maker.Difficulty;
        Changes = changes;
        Description = report.Description;
        StateBefore = stateBefore;
        StateAfter = stateAfter;
        HighlightManager = report.HighlightManager;
        Explanation = report.Explanation;
    }
}