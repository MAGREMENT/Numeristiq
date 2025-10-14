using Model.Sudokus.Player;

namespace Model.YourPuzzles.Syntax.Operators;

public class IsOperator : ISyntaxOperator
{
    public int Priority => 0;
    
    public SyntaxString ToSyntaxString()
    {
        return new SyntaxString("IS", HighlightColor.Third);
    }

    public SyntaxElementType ExpectedOnLeft => SyntaxElementType.Value;
    public SyntaxElementType ExpectedOnRight => SyntaxElementType.Value;
}