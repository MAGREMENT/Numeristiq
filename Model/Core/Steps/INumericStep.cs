using System.Collections.Generic;
using System.Text;
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
        if (Changes.Count == 0) return "";

        var builder = new StringBuilder(Changes[0].ToString());
        for (int i = 1; i < Changes.Count; i++)
        {
            builder.Append($", {Changes[i].ToString()}");
        }

        return builder.ToString();
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
        if (Changes.Count == 0) return "";
        
        var builder = new StringBuilder();
        foreach (var change in Changes)
        {
            builder.Append($"{change.ToString()}, ");
        }

        return builder.ToString()[..^2];
    }

}