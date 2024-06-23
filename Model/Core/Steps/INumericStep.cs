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
    IUpdatableDichotomousSolvingState From { get; }
    IUpdatableDichotomousSolvingState To { get; }
    HighlightManager<THighlighter> HighlightManager { get; }

    string IStep.GetCursorPosition()
    {
        return HighlightManager.CursorPosition();
    }

    string IStep.ChangesToString() => string.Empty; //TODO
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

    string IStep.ChangesToString()
    {
        if (Changes.Count == 0) return "";
        
        var builder = new StringBuilder();
        foreach (var change in Changes)
        {
            var action = change.Type == ChangeType.PossibilityRemoval
                ? "<>"
                : "==";
            builder.Append($"r{change.Row + 1}c{change.Column + 1} {action} {change.Number}, ");
        }

        return builder.ToString()[..^2];
    }

}