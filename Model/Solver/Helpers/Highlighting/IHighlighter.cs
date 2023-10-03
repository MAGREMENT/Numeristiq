namespace Model.Solver.Helpers.Highlighting;

public interface IHighlighter
{
    void Highlight(IHighlightable highlightable);
}

public class DelegateHighlighter : IHighlighter
{
    private readonly Highlight _d;

    public DelegateHighlighter(Highlight d)
    {
        _d = d;
    }

    public void Highlight(IHighlightable highlightable)
    {
        _d(highlightable);
    }
}

public delegate void Highlight(IHighlightable highlightable);