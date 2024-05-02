using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Explanation;

namespace Model.Helpers.Steps;

public interface ISolverStep
{
    int Id { get; }
    string Title { get; }
    StepDifficulty Difficulty { get; }
    string Description { get; }
    ExplanationElement? Explanation { get; }
    string GetCursorPosition();
}

public interface ISolverStep<THighlighter> : ISolverStep
{ 
    IReadOnlyList<SolverProgress> Changes { get; }
    IUpdatableSolvingState From { get; }
    IUpdatableSolvingState To { get; }
    HighlightManager<THighlighter> HighlightManager { get; }

    string ISolverStep.GetCursorPosition()
    {
        return HighlightManager.CursorPosition();
    }
}