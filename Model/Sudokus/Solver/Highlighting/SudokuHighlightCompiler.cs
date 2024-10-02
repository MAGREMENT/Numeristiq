using Model.Core.Highlighting;

namespace Model.Sudokus.Solver.Highlighting;

public class SudokuHighlightCompiler : IHighlightCompiler<ISudokuHighlighter>
{
    public IHighlightable<ISudokuHighlighter> Compile(IHighlightable<ISudokuHighlighter> d)
    {
        return SudokuHighlightExecutable.FromHighlightable(d);
    }
}