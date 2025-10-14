using Model.Sudokus.Player;

namespace Model.YourPuzzles.Syntax.Operators;

public class AddOperator : ISyntaxOperator
{
    public int Priority => 1;
    
    public SyntaxString ToSyntaxString()
    {
        return new SyntaxString("+", HighlightColor.First);
    }
    
    public override string ToString()
    {
        return "+";
    }

    public SyntaxElementType ExpectedOnLeft => SyntaxElementType.Operator | SyntaxElementType.Value;
    public SyntaxElementType ExpectedOnRight => SyntaxElementType.Operator | SyntaxElementType.Value;
}