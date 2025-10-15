using Model.Sudokus.Player;

namespace Model.Core.Syntax.Operators;

public class EqualsOperator : SyntaxOperator
{
    public override int Priority => 0;
    
    protected override HighlightColor GetHighlight() => HighlightColor.Third;
    protected override string GetString() => "=";
}