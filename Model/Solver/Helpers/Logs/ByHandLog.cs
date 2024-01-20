using System;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;

namespace Model.Solver.Helpers.Logs;

public class ByHandLog : ISolverLog
{
    public int Id { get; }
    public string Title { get; }
    public Intensity Intensity => Intensity.Six;
    public string Changes { get; }
    public string Description { get; }
    public SolverState StateBefore { get; }
    public SolverState StateAfter { get; }
    public HighlightManager HighlightManager => new(new DelegateHighlighter(HighLight));
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
                Changes = $"r{row + 1}c{col + 1} <> {possibility}";
                Description = "This possibility was removed by hand";
                break;
            case ChangeType.Solution :
                Title = "Added by hand";
                Changes = $"r{row + 1}c{col + 1} == {possibility}";
                Description = "This solution was added by hand";
                break;
            default: throw new ArgumentException("Invalid change type");
        }
        
        
        _change = new SolverChange(changeType, possibility, row, col);
    }

    private void HighLight(IHighlightable highlightable)
    {
        IChangeReportBuilder.HighlightChange(highlightable, _change);
    }
}