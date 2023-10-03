namespace Model.Solver.Helpers.Highlighting;

public class HighlightManager : IHighlighter
{
    private readonly IHighlighter[] _highlights;
    private int _cursor;

    public int Count => _highlights.Length;

    public HighlightManager(IHighlighter highlight)
    {
        _highlights = new[] { highlight };
    }

    public HighlightManager(params IHighlighter[] highlights)
    {
        _highlights = highlights;
    }

    public void Highlight(IHighlightable highlightable)
    {
        if (Count == 0) return;
        _highlights[_cursor].Highlight(highlightable);
    }

    public void ShiftLeft()
    {
        _cursor--;
        if (_cursor < 0) _cursor += Count;
    }

    public void ShiftRight()
    {
        _cursor = (_cursor + 1) % Count;
    }

    public string CursorPosition()
    {
        return $"{_cursor + 1} / {Count}";
    }
}