using System.Collections.Generic;
using Model.Core.Changes;
using Model.Core.Explanations;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Highlighting;

namespace Model.Core.Steps;

public class ChangeReportStep<THighlighter, TSolvingState, TChange> : IStep<THighlighter, TSolvingState, TChange>
    where TChange : notnull
{
    public string Title { get; }
    public Difficulty Difficulty { get; }
    public string Description { get; }
    public Explanation<THighlighter>? Explanation { get; }
    public IReadOnlyList<TChange> Changes { get; }
    public TSolvingState From { get; }
    public TSolvingState To { get; }
    public HighlightCollection<THighlighter> HighlightCollection { get; }

    public ChangeReportStep(Strategy maker, IReadOnlyList<TChange> changes,
        ChangeReport<THighlighter> report, TSolvingState stateBefore, TSolvingState stateAfter)
    {
        Title = maker.Name;
        Difficulty = maker.Difficulty;
        Description = report.Description;
        Explanation = report.Explanation;
        Changes = changes;
        From = stateBefore;
        To = stateAfter;
        HighlightCollection = report.HighlightCollection;
    }
}