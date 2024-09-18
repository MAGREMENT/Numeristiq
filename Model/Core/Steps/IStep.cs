using System.Collections.Generic;
using Model.Core.Explanations;
using Model.Core.Highlighting;
using Model.Utility.Collections;

namespace Model.Core.Steps;

public interface IStep
{
    int Id { get; }
    string Title { get; }
    Difficulty Difficulty { get; }
    string Description { get; }
    int HighlightCount();
    string ChangesToString();
    string ExplanationToString();
}

public interface IStep<THighlighter, out TState> : IStep
{
    TState From { get; }
    TState To { get; }
    HighlightCollection<THighlighter> HighlightCollection { get; }
    Explanation<THighlighter> Explanation { get; }
    
    int IStep.HighlightCount()
    {
        return HighlightCollection.Count;
    }

    string IStep.ExplanationToString()
    {
        return Explanation.Count == 0 ? "None" : Explanation.FullExplanation();
    }
}

public interface IStep<THighlighter, out TState, out TChange> : IStep<THighlighter, TState> where TChange : notnull
{
    
    IReadOnlyList<TChange> Changes { get; }

    string IStep.ChangesToString()
    {
        return Changes.ToStringSequence(", ");
    }
}