namespace Model.Core.Syntax.Parsables;

public class SimpleStringParsable<T> : ISyntaxParsable where T : ISyntaxElement, new()
{
    public string? IsSimpleString { get; }

    public SimpleStringParsable(string s)
    {
        IsSimpleString = s;
    }
    
    public ISyntaxElement? Parse(string s)
    {
        return s != IsSimpleString ? default : new T();
    }
}