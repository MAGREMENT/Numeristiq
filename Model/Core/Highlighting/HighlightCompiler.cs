using System.Collections.Generic;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Core.Highlighting;

public interface IHighlightCompiler<THighlighter>
{
    public IHighlightable<THighlighter> Compile(IHighlightable<THighlighter> d);
}

public class DefaultHighlightCompiler<THighlighter> : IHighlightCompiler<THighlighter>
{
    public IHighlightable<THighlighter> Compile(IHighlightable<THighlighter> d)
    {
        return d;
    }
}

public class SudokuHighlightCompiler : IHighlightCompiler<ISudokuHighlighter>
{
    public IHighlightable<ISudokuHighlighter> Compile(IHighlightable<ISudokuHighlighter> d)
    {
        return HighlightExecutable.FromHighlightable(d);
    }
}