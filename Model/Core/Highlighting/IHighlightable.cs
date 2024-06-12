namespace Model.Core.Highlighting;

public interface IHighlightable<in THighlighter>
{
    void Highlight(THighlighter highlighter);
}

public class DelegateHighlightable<THighlighter> : IHighlightable<THighlighter>
{
    private readonly Highlight<THighlighter> _d;

    public DelegateHighlightable(Highlight<THighlighter> d)
    {
        _d = d;
    }

    public void Highlight(THighlighter highlighter)
    {
        _d(highlighter);
    }
}

public delegate void Highlight<in THighlighter>(THighlighter highlighter);