using System.Collections.Generic;
using Model.Helpers.Highlighting;
using Model.Sudoku.Solver.Explanation;

namespace Model.Helpers.Changes;

public class ChangeReport
{
    public string Description { get; }
    public ExplanationElement? Explanation { get; }
    public HighlightManager<ISudokuHighlighter> HighlightManager { get; }
    
    public ChangeReport(string description, Highlight<ISudokuHighlighter> highlighter)
    {
        Description = description;
        HighlightManager = new HighlightManager<ISudokuHighlighter>(HighlightCompiler.For<ISudokuHighlighter>().Compile(highlighter));
        Explanation = null;
    }
    
    public ChangeReport(string description, Highlight<ISudokuHighlighter> highlighter, ExplanationElement? explanation)
    {
        Description = description;
        HighlightManager = new HighlightManager<ISudokuHighlighter>(HighlightCompiler.For<ISudokuHighlighter>().Compile(highlighter));
        Explanation = explanation;
    }
    
    public ChangeReport(string description, params Highlight<ISudokuHighlighter>[] highlighters)
    {
        Description = description;

        IHighlightable<ISudokuHighlighter>[] compiled = new IHighlightable<ISudokuHighlighter>[highlighters.Length];

        for (int i = 0; i < highlighters.Length; i++)
        {
            compiled[i] = HighlightCompiler.For<ISudokuHighlighter>().Compile(highlighters[i]);
        }
        
        HighlightManager = new HighlightManager<ISudokuHighlighter>(compiled);
        Explanation = null;
    }
    
    public ChangeReport(string description, Highlight<ISudokuHighlighter> first, params Highlight<ISudokuHighlighter>[] highlighters)
    {
        Description = description;

        var compiled = new IHighlightable<ISudokuHighlighter>[highlighters.Length + 1];
        compiled[0] = HighlightCompiler.For<ISudokuHighlighter>().Compile(first);

        for (int i = 0; i < highlighters.Length; i++)
        {
            compiled[i + 1] = HighlightCompiler.For<ISudokuHighlighter>().Compile(highlighters[i]);
        }
        
        HighlightManager = new HighlightManager<ISudokuHighlighter>(compiled);
        Explanation = null;
    }

    public static ChangeReport Default(IReadOnlyList<SolverProgress> changes)
    {
        return new ChangeReport("",
            lighter => ChangeReportHelper.HighlightChanges(lighter, changes));
    }
}