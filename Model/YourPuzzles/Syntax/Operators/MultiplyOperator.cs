using Model.Sudokus.Player;

namespace Model.YourPuzzles.Syntax.Operators;

public class MultiplyOperator : ISyntaxOperator
{
    public int Priority => 10;
    
    public SyntaxString ToSyntaxString()
    {
        return new SyntaxString("*", HighlightColor.First);
    }
    
    public override string ToString()
    {
        return "*";
    }
}