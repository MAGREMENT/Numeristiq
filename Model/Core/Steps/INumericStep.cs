using System.Collections.Generic;
using Model.Core.Changes;
using Model.Core.Explanation;
using Model.Core.Highlighting;
using Model.Utility.Collections;

namespace Model.Core.Steps;

public interface IStep
{
    int Id { get; }
    string Title { get; }
    StepDifficulty Difficulty { get; }
    string Description { get; }
    ExplanationElement? Explanation { get; }
    string GetCursorPosition();
    string ChangesToString();
}

public interface IDichotomousStep<THighlighter> : IStep
{
    IReadOnlyList<DichotomousChange> Changes { get; }
    IDichotomousSolvingState From { get; }
    IDichotomousSolvingState To { get; }
    HighlightManager<THighlighter> HighlightManager { get; }

    string IStep.GetCursorPosition()
    {
        return HighlightManager.CursorPosition();
    }

    string IStep.ChangesToString()
    {
        return Changes.ToStringSequence(", ");
    }
}

public interface INumericStep<THighlighter> : IStep
{
    IReadOnlyList<NumericChange> Changes { get; }
    INumericSolvingState From { get; }
    INumericSolvingState To { get; }
    HighlightManager<THighlighter> HighlightManager { get; }

    string IStep.GetCursorPosition()
    {
        return HighlightManager.CursorPosition();
    }

    string IStep.ChangesToString()
    {
        return Changes.ToStringSequence(", ");
    }

}