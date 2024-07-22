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
    int HighlightCount();
    string ChangesToString();
}

public interface IStep<THighlighter, out TState> : IStep
{
    TState From { get; }
    TState To { get; }
    HighlightManager<THighlighter> HighlightManager { get; }
}

public interface IDichotomousStep<THighlighter> : IStep<THighlighter, IDichotomousSolvingState>
{
    IReadOnlyList<DichotomousChange> Changes { get; }

    int IStep.HighlightCount()
    {
        return HighlightManager.Count;
    }

    string IStep.ChangesToString()
    {
        return Changes.ToStringSequence(", ");
    }
}

public interface INumericStep<THighlighter> : IStep<THighlighter, INumericSolvingState>
{
    IReadOnlyList<NumericChange> Changes { get; }

    int IStep.HighlightCount()
    {
        return HighlightManager.Count;
    }

    string IStep.ChangesToString()
    {
        return Changes.ToStringSequence(", ");
    }

}