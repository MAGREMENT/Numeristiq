namespace Model.SudokuSolving.Solver.Explanation;

public abstract class ExplanationElement
{
    private ExplanationElement? _next;
    public ExplanationElement? Next => _next;
    
    public abstract bool ShouldBeBold { get; }
    public abstract ExplanationColor Color { get; }

    public ExplanationElement Append(ExplanationElement next)
    {
        _next ??= next;
        return _next;
    }

    public abstract void Show(IExplanationShower shower);
}

public enum ExplanationColor
{
    TextDefault, Primary, Secondary
}