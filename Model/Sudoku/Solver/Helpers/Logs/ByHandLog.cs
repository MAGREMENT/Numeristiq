using System;
using System.Collections.Generic;
using Model.Sudoku.Solver.Explanation;
using Model.Sudoku.Solver.Helpers.Changes;
using Model.Sudoku.Solver.Helpers.Highlighting;

namespace Model.Sudoku.Solver.Helpers.Logs;

public class ByHandLog : ISolverLog
{
    public int Id { get; }
    public string Title { get; }
    public Intensity Intensity => Intensity.Six;
    public IReadOnlyList<SolverChange> Changes => new[] { _change };
    public string Description { get; }
    public ExplanationElement? Explanation => null;
    public SolverState StateBefore { get; }
    public SolverState StateAfter { get; }
    public HighlightManager HighlightManager => new(new DelegateHighlightable(HighLight));
    public bool FromSolving => false;

    private readonly SolverChange _change;

    public ByHandLog(int id, int possibility, int row, int col, ChangeType changeType, SolverState stateBefore, SolverState stateAfter)
    {
        Id = id;
        StateBefore = stateBefore;
        StateAfter = stateAfter;
        switch (changeType)
        {
            case ChangeType.Possibility :
                Title = "Removed by hand";
                Description = "This possibility was removed by hand";
                break;
            case ChangeType.Solution :
                Title = "Added by hand";
                Description = "This solution was added by hand";
                break;
            default: throw new ArgumentException("Invalid change type");
        }
        
        
        _change = new SolverChange(changeType, possibility, row, col);
    }

    private void HighLight(IHighlighter highlighter)
    {
        IChangeReportBuilder.HighlightChange(highlighter, _change);
    }
}