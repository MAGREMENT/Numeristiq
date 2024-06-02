using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver.Explanation;

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

public interface IStep<THighlighter> : IStep
{ 
    IReadOnlyList<SolverProgress> Changes { get; }
    IUpdatableSolvingState From { get; }
    IUpdatableSolvingState To { get; }
    HighlightManager<THighlighter> HighlightManager { get; }

    string IStep.GetCursorPosition()
    {
        return HighlightManager.CursorPosition();
    }
}