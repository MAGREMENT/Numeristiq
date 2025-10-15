using Model.Sudokus.Player;
namespace Model.Core.Syntax.Operators;

public class AddOperator : SyntaxOperator
{
    public override int Priority => 1;
    protected override HighlightColor GetHighlight() => HighlightColor.First;
    protected override string GetString() => "+";
}