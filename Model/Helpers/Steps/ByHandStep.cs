using System;
using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver.Explanation;

namespace Model.Helpers.Steps;

public class ByHandStep<THighlighter> : ISolverStep<THighlighter> where THighlighter : ISolvingStateHighlighter
{
    public int Id { get; }
    public string Title { get; }
    public Intensity Intensity => Intensity.Six;
    public IReadOnlyList<SolverProgress> Changes => new[] { _progress };
    public string Description { get; }
    public ExplanationElement? Explanation => null;
    public IUpdatableSolvingState From { get; }
    public IUpdatableSolvingState To { get; }
    public HighlightManager<THighlighter> HighlightManager => new(new DelegateHighlightable<THighlighter>(HighLight));
    public bool FromSolving => false;

    private readonly SolverProgress _progress;

    public ByHandStep(int id, int possibility, int row, int col, ProgressType progressType, IUpdatableSolvingState stateBefore)
    {
        Id = id;
        From = stateBefore;
        switch (progressType)
        {
            case ProgressType.PossibilityRemoval :
                Title = "Removed by hand";
                Description = "This possibility was removed by hand";
                break;
            case ProgressType.SolutionAddition :
                Title = "Added by hand";
                Description = "This solution was added by hand";
                break;
            default: throw new ArgumentException("Invalid change type");
        }
        
        
        _progress = new SolverProgress(progressType, possibility, row, col);
        To = stateBefore.Apply(_progress);
    }

    private void HighLight<TH>(TH highlighter) where TH : ISolvingStateHighlighter
    {
        ChangeReportHelper.HighlightChange(highlighter, _progress);
    }
}