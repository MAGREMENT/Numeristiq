using Model.Sudokus.Player;

namespace Model.Core.Syntax.Operators;

public class PowerOperator : SyntaxOperator
{
    public override int Priority => 100;
    
    protected override HighlightColor GetHighlight() => HighlightColor.First;
    protected override string GetString() => "^";
}