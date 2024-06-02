using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver.Explanation;

namespace Model.Core.Steps;

public class ChangeReportStep<THighlighter> : IStep<THighlighter>
{
    public int Id { get; }
    public string Title { get; }
    public StepDifficulty Difficulty { get; }
    public IReadOnlyList<SolverProgress> Changes { get; }
    public string Description { get; }
    public ExplanationElement? Explanation { get; }
    public IUpdatableSolvingState From { get; }
    public IUpdatableSolvingState To { get; }
    public HighlightManager<THighlighter> HighlightManager  { get; }


    public ChangeReportStep(int id, Strategy maker, IReadOnlyList<SolverProgress> changes, ChangeReport<THighlighter> report,
        IUpdatableSolvingState stateBefore)
    {
        Id = id;
        Title = maker.Name;
        Difficulty = maker.Difficulty;
        Changes = changes;
        Description = report.Description;
        From = stateBefore;
        To = stateBefore.Apply(changes);
        HighlightManager = report.HighlightManager;
        Explanation = report.Explanation;
    }
}