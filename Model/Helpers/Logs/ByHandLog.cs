using System;
using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Explanation;

namespace Model.Helpers.Logs;

public class ByHandLog<THighlighter> : ISolverLog<THighlighter>
{
    public int Id { get; }
    public string Title { get; }
    public Intensity Intensity => Intensity.Six;
    public IReadOnlyList<SolverProgress> Changes => new[] { _progress };
    public string Description { get; }
    public ExplanationElement? Explanation => null;
    public IUpdatableSolvingState StateBefore { get; }
    public IUpdatableSolvingState StateAfter { get; }
    public HighlightManager<THighlighter> HighlightManager => new(new DelegateHighlightable<THighlighter>(HighLight));
    public bool FromSolving => false;

    private readonly SolverProgress _progress;

    public ByHandLog(int id, int possibility, int row, int col, ProgressType progressType, IUpdatableSolvingState stateBefore)
    {
        Id = id;
        StateBefore = stateBefore;
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
        StateAfter = stateBefore.Apply(_progress);
    }

    private void HighLight<TH>(TH highlighter)
    {
        //ChangeReportHelper.HighlightChange(highlighter, _progress); TODO
    }
}