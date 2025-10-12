using Model.Utility.Collections;

namespace Model.YourPuzzles.Syntax;

public interface ISyntaxTranslatable
{
    public BinaryTreeNode<ISyntaxElement> ToSyntax();
}