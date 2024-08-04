using System.Collections.Generic;
using Model.Core.Changes;
using Model.Core.Explanation;
using Model.Core.Highlighting;

namespace Model.Core.Steps;

public class ChangeReportBinaryStep<THighlighter> : IBinaryStep<THighlighter>
{
    public int Id { get; }
    public string Title { get; }
    public Difficulty Difficulty { get; }
    public string Description { get; }
    public ExplanationElement? Explanation { get; }
    public IReadOnlyList<BinaryChange> Changes { get; }
    public IBinarySolvingState From { get; }
    public IBinarySolvingState To { get; }
    public HighlightManager<THighlighter> HighlightManager { get; }

    public ChangeReportBinaryStep(int id, Strategy maker, IReadOnlyList<BinaryChange> changes,
        ChangeReport<THighlighter> report, IBinarySolvingState stateBefore, IBinarySolvingState stateAfter)
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