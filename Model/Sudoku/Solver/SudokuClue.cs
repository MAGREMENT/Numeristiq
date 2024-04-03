using Model.Helpers.Highlighting;

namespace Model.Sudoku.Solver;

public class SudokuClue : IHighlightable<ISudokuHighlighter>
{
    private readonly IHighlightable<ISudokuHighlighter>? _highlightable;
    public string Text { get; }
    
    public SudokuClue(Highlight<ISudokuHighlighter> highlight, string text)
    {
        Text = text;
        _highlightable = HighlightCompiler.ForSudoku.Compile(highlight);
    }

    public SudokuClue(string text)
    {
        Text = text;
        _highlightable = null;
    }

    public void Highlight(ISudokuHighlighter highlighter)
    {
        _highlightable?.Highlight(highlighter);
    }
}