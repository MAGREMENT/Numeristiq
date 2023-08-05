namespace Model;

public class ChangeReport
{
    public string Explanation { get; }

    public HighLightCause CauseHighLighter { get; }
    
    public string Changes { get; }
    
    public ChangeReport(string changes, HighLightCause causeHighLighter, string explanation)
    {
        Explanation = explanation;
        CauseHighLighter = causeHighLighter;
        Changes = changes;
    }
}