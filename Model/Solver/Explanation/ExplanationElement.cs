namespace Model.Solver.Explanation;

public abstract class ExplanationElement
{
    private ExplanationElement? _next;
    public ExplanationElement? Next => _next;

    public ExplanationElement Append(ExplanationElement next)
    {
        _next ??= next;
        return _next;
    }
}