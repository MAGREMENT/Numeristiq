using System.Collections.Generic;
using Model.Core.Changes;
using Model.Core.Explanation;
using Model.Core.Highlighting;

namespace Model.Core.Steps;

public class ChangeReportDichotomousStep<THighlighter> : IDichotomousStep<THighlighter>
{
    public int Id { get; }
    public string Title { get; }
    public StepDifficulty Difficulty { get; }
    public string Description { get; }
    public ExplanationElement? Explanation { get; }
    public IReadOnlyList<DichotomousChange> Changes { get; }
    public IDichotomousSolvingState From { get; }
    public IDichotomousSolvingState To { get; }
    public HighlightManager<THighlighter> HighlightManager { get; }

    public ChangeReportDichotomousStep(int id, Strategy maker, IReadOnlyList<DichotomousChange> changes,
        ChangeReport<THighlighter> report, IDichotomousSolvingState stateBefore, IDichotomousSolvingState stateAfter)
    {
        Id = id;
        Title = maker.Name;
        Difficulty = maker.Difficulty;
        Description = report.Description;
        Explanation = report.Explanation;
        Changes = changes;
        From = stateBefore;
        To = stateAfter;
        HighlightManager = report.HighlightManager;
    }
}