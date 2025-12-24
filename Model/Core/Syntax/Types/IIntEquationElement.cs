namespace Model.Core.Syntax.Types;

public interface IIntEquationSyntaxElement : ISyntaxElement
{
    int ComputeValue();
}