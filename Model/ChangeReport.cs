namespace Model;

public class ChangeReport
{
    public string Explanation { get; }

    public HighLightSolver SolverHighLighter { get; }
    
    public string Changes { get; }
    
    public ChangeReport(string changes, HighLightSolver solverHighLighter, string explanation)
    {
        Explanation = explanation;
        SolverHighLighter = solverHighLighter;
        Changes = changes;
    }
}