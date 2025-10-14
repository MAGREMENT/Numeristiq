using Model.Sudokus.Player;

namespace Model.YourPuzzles.Syntax.Operators;

public class EqualsOperator : ISyntaxOperator
{
    public int Priority => 0;

    public SyntaxString ToSyntaxString()
    {
        return new SyntaxString("=", HighlightColor.Third);
    }
    
    public override string ToString()
    {
        return "=";
    }

    public SyntaxElementType ExpectedOnLeft => SyntaxElementType.Operator | SyntaxElementType.Value;
    public SyntaxElementType ExpectedOnRight => SyntaxElementType.Operator | SyntaxElementType.Value;
}