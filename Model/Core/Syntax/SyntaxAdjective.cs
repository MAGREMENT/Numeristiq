namespace Model.Core.Syntax;

public abstract class SyntaxAdjective : SyntaxElement
{
    public ISyntaxElement? Child { get; set; } = null;
    
    public override string ToString()
    {
        return Child is null ? GetString() : GetString() + " " + Child;
    }

    public override bool TrySetNext(ISyntaxElement el)
    {
        throw new System.NotImplementedException();
    }

    public override bool TryForceAppend(ISyntaxElement el)
    {
        if (Child is not null) return false;

        Child = el;
        return true;
    }
}