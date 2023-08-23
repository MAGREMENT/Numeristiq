namespace Model;

public class ChangeReport
{
    public string Explanation { get; }
    public HighlightManager HighlightManager { get; }
    public string Changes { get; }
    
    public ChangeReport(string changes, HighlightSolver solverHighLighter, string explanation)
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
}