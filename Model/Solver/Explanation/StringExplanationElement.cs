namespace Model.Solver.Explanation;

public class StringExplanationElement : ExplanationElement
{
    private readonly string _value;
    
    public StringExplanationElement(string value)
    {
        _value = value;
    }

    public override string ToString()
    {
        return _value;
    }
}