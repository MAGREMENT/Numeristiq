namespace Model.Sudoku.Solver.Helpers.Highlighting;

public interface IHighlightable
{
    void Highlight(IHighlighter highlighter);
}

public class DelegateHighlightable : IHighlightable
{
    private readonly Highlight _d;

    public DelegateHighlightable(Highlight d)
    {
        _d = d;
    }

    public void Highlight(IHighlighter highlighter)
    {
        _d(highlighter);
    }
}

public delegate void Highlight(IHighlighter highlighter);