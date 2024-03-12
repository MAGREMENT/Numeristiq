using System.Collections.Generic;
using Model.Helpers.Highlighting;
using Model.Sudoku.Solver.Explanation;

namespace Model.Helpers.Changes;

public class ChangeReport<THighlighter>
{
    public string Description { get; }
    public ExplanationElement? Explanation { get; }
    public HighlightManager<THighlighter> HighlightManager { get; }
    
    public ChangeReport(string description, Highlight<THighlighter> highlighter)
    {
        Description = description;
        HighlightManager = new HighlightManager<THighlighter>(HighlightCompiler.For<THighlighter>().Compile(highlighter));
        Explanation = null;
    }
    
    public ChangeReport(string description, Highlight<THighlighter> highlighter, ExplanationElement? explanation)
    {
        Description = description;
        HighlightManager = new HighlightManager<THighlighter>(HighlightCompiler.For<THighlighter>().Compile(highlighter));
        Explanation = explanation;
    }
    
    public ChangeReport(string description, params Highlight<THighlighter>[] highlighters)
    {
        Description = description;

        var compiled = new IHighlightable<THighlighter>[highlighters.Length];

        for (int i = 0; i < highlighters.Length; i++)
        {
            compiled[i] = HighlightCompiler.For<THighlighter>().Compile(highlighters[i]);
        }
        
        HighlightManager = new HighlightManager<THighlighter>(compiled);
        Explanation = null;
    }
    
    public ChangeReport(string description, Highlight<THighlighter> first, params Highlight<THighlighter>[] highlighters)
    {
        Description = description;

        var compiled = new IHighlightable<THighlighter>[highlighters.Length + 1];
        compiled[0] = HighlightCompiler.For<THighlighter>().Compile(first);

        for (int i = 0; i < highlighters.Length; i++)
        {
            compiled[i + 1] = HighlightCompiler.For<THighlighter>().Compile(highlighters[i]);
        }
        
        HighlightManager = new HighlightManager<THighlighter>(compiled);
        Explanation = null;
    }
}