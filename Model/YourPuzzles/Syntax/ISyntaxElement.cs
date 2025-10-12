using Model.Sudokus.Player;

namespace Model.YourPuzzles.Syntax;

public interface ISyntaxElement
{
    SyntaxElementType Type { get; }
    
    public int Priority { get; }
    
    SyntaxString ToSyntaxString();
}

public enum SyntaxElementType
{
    Value, Operator
}

public readonly struct SyntaxString
{
    public readonly string value;
    public readonly HighlightColor color;

    public SyntaxString(string value, HighlightColor color)
    {
        this.value = value;
        this.color = color;
    }
}