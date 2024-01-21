namespace Model.Solver.Helpers.Highlighting;

public class HighlightManager : IHighlightable
{
    private readonly IHighlightable[] _highlights;
    private int _cursor;

    public int Count => _highlights.Length;

    public HighlightManager(IHighlightable highlight)
    {
        _highlights = new[] { highlight };
    }

    public HighlightManager(params IHighlightable[] highlights)
    {
        _highlights = highlights;
    }

    public void Highlight(IHighlighter highlighter)
    {
        if (Count == 0) return;
        _highlights[_cursor].Highlight(highlighter);
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