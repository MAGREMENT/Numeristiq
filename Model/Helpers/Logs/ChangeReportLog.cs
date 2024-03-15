using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudoku.Solver.Explanation;

namespace Model.Helpers.Logs;

public class ChangeReportLog<THighlighter> : ISolverLog<THighlighter>
{
    public int Id { get; }
    public string Title { get; }
    public Intensity Intensity { get; }
    public IReadOnlyList<SolverProgress> Changes { get; }
    public string Description { get; }
    public ExplanationElement? Explanation { get; }
    public IUpdatableSolvingState StateBefore { get; }
    public IUpdatableSolvingState StateAfter { get; }
    public HighlightManager<THighlighter> HighlightManager  { get; }
    public bool FromSolving => true;


    public ChangeReportLog(int id, ICommitMaker maker, IReadOnlyList<SolverProgress> changes, ChangeReport<THighlighter> report,
        IUpdatableSolvingState stateBefore)
    {
        Id = id;
        Title = maker.Name;
        Intensity = (Intensity)maker.Difficulty;
        Changes = changes;
        Description = report.Description;
        StateBefore = stateBefore;
        StateAfter = stateBefore.Apply(changes);
        HighlightManager = report.HighlightManager;
        Explanation = report.Explanation;
    }
}