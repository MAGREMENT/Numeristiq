namespace Model.YourPuzzles.Syntax;

public interface ISyntaxOperator : ISyntaxElement
{
    SyntaxElementType ISyntaxElement.Type => SyntaxElementType.Operator;

    SyntaxElementType ExpectedOnLeft { get; }
    SyntaxElementType ExpectedOnRight { get; }
}