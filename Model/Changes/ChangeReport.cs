using System.Collections.Generic;

namespace Model.Changes;

public class ChangeReport
{
    public string Explanation { get; }
    public HighlightManager HighlightManager { get; }
    public string Changes { get; }
    
    public ChangeReport(string changes,  string explanation, HighlightSolver solverHighLighter)
    {
        Explanation = explanation;
        Changes = changes;
        HighlightManager = new HighlightManager(solverHighLighter);
    }
    
    public ChangeReport(string changes, string explanation, params HighlightSolver[] solverHighLighter)
    {
        Explanation = explanation;
        Changes = changes;
        HighlightManager = new HighlightManager(solverHighLighter);
    }

    public static ChangeReport Default(List<SolverChange> changes)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "",
            lighter => IChangeReportBuilder.HighlightChanges(lighter, changes));
    }
}