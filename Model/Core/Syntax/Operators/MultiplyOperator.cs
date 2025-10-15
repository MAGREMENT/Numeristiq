using Model.Sudokus.Player;

namespace Model.Core.Syntax.Operators;

public class MultiplyOperator : SyntaxOperator
{
    public override int Priority => 10;
    
    protected override HighlightColor GetHighlight() => HighlightColor.First;
    protected override string GetString() => "*";
}