using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver.Explanation;

namespace Model.Helpers.Steps;

public class ChangeReportStep<THighlighter> : ISolverStep<THighlighter>
{
    public int Id { get; }
    public string Title { get; }
    public Intensity Intensity { get; }
    public IReadOnlyList<SolverProgress> Changes { get; }
    public string Description { get; }
    public ExplanationElement? Explanation { get; }
    public IUpdatableSolvingState From { get; }
    public IUpdatableSolvingState To { get; }
    public HighlightManager<THighlighter> HighlightManager  { get; }
    public bool FromSolving => true;


    public ChangeReportStep(int id, ICommitMaker maker, IReadOnlyList<SolverProgress> changes, ChangeReport<THighlighter> report,
        IUpdatableSolvingState stateBefore)
    {
        Id = id;
        Title = maker.Name;
        Intensity = (Intensity)maker.Difficulty;
        Changes = changes;
        Description = report.Description;
        From = stateBefore;
        To = stateBefore.Apply(changes);
        HighlightManager = report.HighlightManager;
        Explanation = report.Explanation;
    }
}