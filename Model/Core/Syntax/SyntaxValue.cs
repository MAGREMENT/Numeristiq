namespace Model.Core.Syntax;

public abstract class SyntaxValue : SyntaxElement
{
    public override int Priority => int.MaxValue;
    public override string ToString() => GetString();
    public override bool TrySetNext(ISyntaxElement el) => false;
    public override bool TryForceAppend(ISyntaxElement el) => false;
}