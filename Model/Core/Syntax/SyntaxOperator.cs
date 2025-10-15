using System.Text;

namespace Model.Core.Syntax;

public abstract class SyntaxOperator : SyntaxElement
{
    public ISyntaxElement? Left { get; set; }
    public ISyntaxElement? Right { get; set; }
    
    public override string ToString()
    {
        var builder = new StringBuilder();

        if (Left is not null)
        {
            if (Left.Priority < Priority) builder.Append('(');
            builder.Append(Left);
            if (Left.Priority < Priority) builder.Append(')');
            builder.Append(' ');
        }

        builder.Append(GetString());

        if (Right is not null)
        {
            builder.Append(' ');
            if (Right.Priority < Priority) builder.Append('(');
            builder.Append(Right);
            if (Right.Priority < Priority) builder.Append(')');
        }

        return builder.ToString();
    }

    public override bool TrySetNext(ISyntaxElement el)
    {
        if (el.Priority < Priority) return false;
        
        if (Left is null)
        {
            Left = el;
            return true;
        }

        if (Right is null)
        {
            Right = el;
            return true;
        }

        if (Right.TrySetNext(el)) return true;

        if (!el.TrySetNext(Right)) return false;

        Right = el;
        return true;
    }

    public override bool TryForceAppend(ISyntaxElement el)
    {
        if (Left is null)
        {
            Left = el;
            return true;
        }

        if (Right is null)
        {
            Right = el;
            return true;
        }

        return false;
    }
}