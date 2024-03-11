namespace Model.Helpers.Highlighting;

public interface IHighlightable<in THighlighter>
{
    void Highlight(THighlighter highlighter);
}

public class DelegateHighlightable : IHighlightable<ISudokuHighlighter>
{
    private readonly Highlight _d;

    public DelegateHighlightable(Highlight d)
    {
        _d = d;
    }

    public void Highlight(ISudokuHighlighter highlighter)
    {
        _d(highlighter);
    }
}

public delegate void Highlight(ISudokuHighlighter highlighter);