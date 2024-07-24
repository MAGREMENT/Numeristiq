using System.Text;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility;
using Model.Utility;

namespace Model.Core.Explanation;

public abstract class ExplanationElement
{
    public ExplanationElement? Next { get; private set; }

    public abstract bool ShouldBeBold { get; }
    public abstract ExplanationColor Color { get; }

    public abstract void Show(IExplanationHighlighter highlighter);
    public abstract bool DoesShowSomething { get; }

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

    public ExplanationElement Append(House ch) => Append(new CoverHouseExplanationElement(ch));

    public ExplanationElement Append(CellPossibility cp) => Append(new CellPossibilityExplanationElement(cp));

    public ExplanationElement Append(Cell cell) => Append(new CellExplanationElement(cell));

    public ExplanationElement Append(params CellPossibility[] cps) =>
        Append(new MultiCellPossibilityExplanationElement(cps));

    public ExplanationElement Append(IPossibilitySet als) => Append(new AlmostLockedSetExplanationElement(als));

    public static ExplanationElement operator +(ExplanationElement element, ExplanationElement s) => element.Append(s);
    public static ExplanationElement operator +(ExplanationElement element, Cell s) => element.Append(s);
    public static ExplanationElement operator +(ExplanationElement element, CellPossibility s) => element.Append(s);
    public static ExplanationElement operator +(ExplanationElement element, House s) => element.Append(s);
    public static ExplanationElement operator +(ExplanationElement element, string s) => element.Append(s);
    public static ExplanationElement operator +(ExplanationElement element, CellPossibility[] s) => element.Append(s);
}

public enum ExplanationColor
{
    TextDefault, Primary, Secondary
}