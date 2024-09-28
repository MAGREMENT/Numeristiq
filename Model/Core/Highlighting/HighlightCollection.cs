using Model.Utility;

namespace Model.Core.Highlighting;

public class HighlightCollection<THighlighter> : IHighlightable<THighlighter>
{
    private readonly IHighlightable<THighlighter>[] _highlights;
    private int _cursor;

    public int Count => _highlights.Length;

    public HighlightCollection(Highlight<THighlighter> highlight)
    {
        _highlights = new IHighlightable<THighlighter>[] { new DelegateHighlightable<THighlighter>(highlight) };
    }

    public HighlightCollection(params Highlight<THighlighter>[] highlights)
    {
        _highlights = new IHighlightable<THighlighter>[highlights.Length];
        for (int i = 0; i < highlights.Length; i++)
        {
            _highlights[i] = new DelegateHighlightable<THighlighter>(highlights[i]);
        }
    }

    public void Compile(IHighlightCompiler<THighlighter> compiler)
    {
        for (int i = 0; i < _highlights.Length; i++)
        {
            _highlights[i] = compiler.Compile(_highlights[i]);
        }
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
        return _highlights[_cursor] is not HighlightExecutable exe 
            ? string.Empty 
            : exe.ToBase16(DefaultBase16Alphabet.Instance);
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