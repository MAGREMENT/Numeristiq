using Model.Sudokus.Player;

namespace Model.YourPuzzles.Syntax.Operators;

public class GreaterThanOperator : ISyntaxOperator
{
    public SyntaxString ToSyntaxString()
    {
        return new SyntaxString(">", HighlightColor.First);
    }
}