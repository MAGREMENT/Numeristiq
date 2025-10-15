namespace Model.Core.Syntax;

public interface ISyntaxParsable<out T> where T : ISyntaxElement
{
    public string? IsSimpleString { get; }
    public T? Parse(string s);
}