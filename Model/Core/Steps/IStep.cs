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
    Difficulty Difficulty { get; }
    string Description { get; }
    ExplanationElement? Explanation { get; }
    int HighlightCount();
    string ChangesToString();
}

public interface IStep<THighlighter, out TState> : IStep //TODO generalize more
{
    TState From { get; }
    TState To { get; }
    HighlightManager<THighlighter> HighlightManager { get; }
    
    int IStep.HighlightCount()
    {
        return HighlightManager.Count;
    }
}

public interface IDichotomousStep<THighlighter> : IStep<THighlighter, IDichotomousSolvingState>
{
    IReadOnlyList<DichotomousChange> Changes { get; }

    string IStep.ChangesToString()
    {
        return Changes.ToStringSequence(", ");
    }
}

public interface INumericStep<THighlighter> : IStep<THighlighter, INumericSolvingState>
{
    IReadOnlyList<NumericChange> Changes { get; }

    string IStep.ChangesToString()
    {
        return Changes.ToStringSequence(", ");
    }
}

public interface IBinaryStep<THighlighter> : IStep<THighlighter, IBinarySolvingState>
{
    IReadOnlyList<BinaryChange> Changes { get; }

    string IStep.ChangesToString()
    {
        return Changes.ToStringSequence(", ");
    }
}