using System.Collections.Generic;
using Model.Core.Changes;
using Model.Core.Explanation;
using Model.Core.Highlighting;

namespace Model.Core.Steps;

public interface IStep
{
    int Id { get; }
    string Title { get; }
    StepDifficulty Difficulty { get; }
    string Description { get; }
    ExplanationElement? Explanation { get; }
    string GetCursorPosition();
}

public interface IDichotomousStep<THighlighter> : IStep
{
    IReadOnlyList<DichotomousChange> Changes { get; }
    IUpdatableDichotomousSolvingState From { get; }
    IUpdatableDichotomousSolvingState To { get; }
    HighlightManager<THighlighter> HighlightManager { get; }

    string IStep.GetCursorPosition()
    {
        return HighlightManager.CursorPosition();
    }
}

public interface INumericStep<THighlighter> : IStep
{ 
    IReadOnlyList<NumericChange> Changes { get; }
    IUpdatableNumericSolvingState From { get; }
    IUpdatableNumericSolvingState To { get; }
    HighlightManager<THighlighter> HighlightManager { get; }

    string IStep.GetCursorPosition()
    {
        return HighlightManager.CursorPosition();
    }
}