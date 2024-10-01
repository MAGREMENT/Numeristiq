using Model.Core.Highlighting;

namespace Model.Sudokus.Solver;

public class SudokuHighlightCompiler : IHighlightCompiler<ISudokuHighlighter>
{
    public IHighlightable<ISudokuHighlighter> Compile(IHighlightable<ISudokuHighlighter> d)
    {
        return HighlightExecutable.FromHighlightable(d);
    }
}