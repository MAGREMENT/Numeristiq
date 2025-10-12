using System;

namespace Model.YourPuzzles.Syntax;

public interface ISyntaxParsable
{
    public string? IsSimpleString { get; }
    public ISyntaxElement? Parse(string s);
}