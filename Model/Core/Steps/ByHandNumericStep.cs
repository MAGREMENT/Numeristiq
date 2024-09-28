using System;
using System.Collections.Generic;
using Model.Core.Changes;
using Model.Core.Explanations;
using Model.Core.Highlighting;

namespace Model.Core.Steps;

public class ByHandNumericStep<THighlighter, TSolvingState> : IStep<THighlighter, TSolvingState, NumericChange>
    where THighlighter : INumericSolvingStateHighlighter
{
    public int Id { get; }
    public string Title { get; }
    public Difficulty Difficulty => Difficulty.None;
    public IReadOnlyList<NumericChange> Changes => new[] { _progress };
    public string Description { get; }
    public Explanation<THighlighter> Explanation => Explanation<THighlighter>.Empty;
    public TSolvingState From { get; }
    public TSolvingState To { get; }
    public HighlightCollection<THighlighter> HighlightCollection => new(DoHighlight);

    private readonly NumericChange _progress;

    public ByHandNumericStep(int id, int possibility, int row, int col, ChangeType changeType, TSolvingState stateBefore,
        TSolvingState stateAfter)
    {
        Id = id;
        From = stateBefore;
        switch (changeType)
        {
            case ChangeType.PossibilityRemoval :
                Title = "Removed by hand";
                Description = "This possibility was removed by hand";
                break;
            case ChangeType.SolutionAddition :
                Title = "Added by hand";
                Description = "This solution was added by hand";
                break;
            default: throw new ArgumentException("Invalid change type");
        }
        
        
        _progress = new NumericChange(changeType, possibility, row, col);
        To = stateAfter;
    }

    private void DoHighlight<TH>(TH highlighter) where TH : INumericSolvingStateHighlighter
    {
        ChangeReportHelper.HighlightChange(highlighter, _progress);
    }
}