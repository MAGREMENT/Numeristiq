using System;

namespace Model.YourPuzzles.Syntax.Parsables;

public class SimpleStringParsable<T> : ISyntaxParsable where T : ISyntaxElement, new()
{
    public string? IsSimpleString { get; }

    public SimpleStringParsable(string s)
    {
        IsSimpleString = s;
    }
    
    public ISyntaxElement? Parse(string s)
    {
        if (s != IsSimpleString) return null;

        return new T();
    }
}