using System;
using System.Collections.Generic;
using Model.Core.Changes;
using Model.Core.Explanation;
using Model.Core.Highlighting;

namespace Model.Core.Steps;

public class ByHandNumericStep<THighlighter> : INumericStep<THighlighter> where THighlighter : INumericSolvingStateHighlighter
{
    public int Id { get; }
    public string Title { get; }
    public StepDifficulty Difficulty => StepDifficulty.None;
    public IReadOnlyList<NumericChange> Changes => new[] { _progress };
    public string Description { get; }
    public ExplanationElement? Explanation => null;
    public IUpdatableNumericSolvingState From { get; }
    public IUpdatableNumericSolvingState To { get; }
    public HighlightManager<THighlighter> HighlightManager => new(new DelegateHighlightable<THighlighter>(HighLight));

    private readonly NumericChange _progress;

    public ByHandNumericStep(int id, int possibility, int row, int col, ChangeType changeType, IUpdatableNumericSolvingState stateBefore)
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
        To = stateBefore.Apply(_progress);
    }

    private void HighLight<TH>(TH highlighter) where TH : INumericSolvingStateHighlighter
    {
        ChangeReportHelper.HighlightChange(highlighter, _progress);
    }
}