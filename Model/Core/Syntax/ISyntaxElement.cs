using Model.Sudokus.Player;

namespace Model.Core.Syntax;

public interface ISyntaxElement
{
    public int Priority { get; }
    
    SyntaxString ToSyntaxString();

    bool TrySetNext(ISyntaxElement el);

    bool TryForceAppend(ISyntaxElement el);
}

public abstract class SyntaxElement : ISyntaxElement
{
    public abstract int Priority { get; }
    
    protected abstract HighlightColor GetHighlight();
    protected abstract string GetString();
    public abstract bool TrySetNext(ISyntaxElement el);
    public abstract bool TryForceAppend(ISyntaxElement el);

    public SyntaxString ToSyntaxString()
    {
        return new SyntaxString(GetString(), GetHighlight());
    }
}

public readonly struct SyntaxString
{
    public readonly string value;
    public readonly HighlightColor color;

    public SyntaxString(string value, HighlightColor color)
    {
        this.value = value;
        this.color = color;
    }
}