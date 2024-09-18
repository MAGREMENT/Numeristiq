namespace Model.Core.Highlighting;

public class HighlightCollection<THighlighter> : IHighlightable<THighlighter>
{
    private readonly IHighlightable<THighlighter>[] _highlights;
    private int _cursor;

    public int Count => _highlights.Length;

    public HighlightCollection(IHighlightable<THighlighter> highlight)
    {
        _highlights = new[] { highlight };
    }

    public HighlightCollection(params IHighlightable<THighlighter>[] highlights)
    {
        _highlights = highlights;
    }

    public void Highlight(THighlighter highlighter)
    {
        if (Count == 0) return;
        _highlights[_cursor].Highlight(highlighter);
    }

    public void GoTo(int pos)
    {
        _cursor = pos;
    }

    public string TryGetInstructionsAsString()
    {
        return _highlights[_cursor] is not SudokuHighlightExecutable exe ? string.Empty : exe.InstructionsAsString();
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