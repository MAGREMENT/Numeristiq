using System.Text;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;

namespace Model.Sudoku.Solver.Explanation;

public abstract class ExplanationElement
{
    public ExplanationElement? Next { get; private set; }

    public abstract bool ShouldBeBold { get; }
    public abstract ExplanationColor Color { get; }

    public abstract void Show(IExplanationHighlighter highlighter);

    public string FullExplanation()
    {
        StringBuilder builder = new(ToString());

        var current = Next;
        while (current is not null)
        {
            builder.Append($"{current}");
            current = current.Next;
        }

        return builder.ToString();
    }
    
    public ExplanationElement Append(ExplanationElement next)
    {
        Next ??= next;
        return Next;
    }

    public ExplanationElement Append(string s) => Append(new StringExplanationElement(s));

    public ExplanationElement Append(CoverHouse ch) => Append(new CoverHouseExplanationElement(ch));

    public ExplanationElement Append(CellPossibility cp) => Append(new CellPossibilityExplanationElement(cp));

    public ExplanationElement Append(Cell cell) => Append(new CellExplanationElement(cell));

    public static ExplanationElement operator +(ExplanationElement element, ExplanationElement s) => element.Append(s);
    public static ExplanationElement operator +(ExplanationElement element, Cell s) => element.Append(s);
    public static ExplanationElement operator +(ExplanationElement element, CellPossibility s) => element.Append(s);
    public static ExplanationElement operator +(ExplanationElement element, CoverHouse s) => element.Append(s);
    public static ExplanationElement operator +(ExplanationElement element, string s) => element.Append(s);
}

public enum ExplanationColor
{
    TextDefault, Primary, Secondary
}