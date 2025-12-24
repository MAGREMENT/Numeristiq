namespace Model.Core.Syntax;

public interface ISyntaxParsable
{
    public string? IsSimpleString { get; }
    public ISyntaxElement? Parse(string s);
}