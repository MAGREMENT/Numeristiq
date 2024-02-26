using System.Text;

namespace Model.Sudoku.Solver.Explanation;

public abstract class ExplanationElement
{
    public ExplanationElement? Next { get; private set; }

    public abstract bool ShouldBeBold { get; }
    public abstract ExplanationColor Color { get; }

    public ExplanationElement Append(ExplanationElement next)
    {
        Next ??= next;
        return Next;
    }

    public abstract void Show(IExplanationHighlighter highlighter);

    public string FullExplanation()
    {
        StringBuilder builder = new(ToString());

        var current = Next;
        while (current is not null)
        {
            builder.Append($" {current}");
            current = current.Next;
        }

        return builder.ToString();
    }
}

public enum ExplanationColor
{
    TextDefault, Primary, Secondary
}