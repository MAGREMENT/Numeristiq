namespace Model.YourPuzzles.Syntax;

public interface ISyntaxValue : ISyntaxElement
{
    int ISyntaxElement.Priority => int.MinValue;
    
    SyntaxElementType ISyntaxElement.Type => SyntaxElementType.Value;
}