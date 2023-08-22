namespace Model;

public class ChangeReport
{
    public string Explanation { get; }
    public HighlightSolver SolverHighLighter { get; }
    public string Changes { get; }
    
    public ChangeReport(string changes, HighlightSolver solverHighLighter, string explanation)
    {
        Explanation = explanation;
        SolverHighLighter = solverHighLighter;
        Changes = changes;
    }
}