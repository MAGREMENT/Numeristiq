using System.Collections.Generic;
using Model.Helpers.Highlighting;
using Model.Sudoku.Solver.Explanation;

namespace Model.Helpers.Changes;

public class ChangeReport
{
    public string Description { get; }
    public ExplanationElement? Explanation { get; }
    public HighlightManager HighlightManager { get; }
    
    public ChangeReport(string description, Highlight highlighter)
    {
        Description = description;
        HighlightManager = new HighlightManager(HighlightCompiler.GetInstance().Compile(highlighter));
        Explanation = null;
    }
    
    public ChangeReport(string description, Highlight highlighter, ExplanationElement? explanation)
    {
        Description = description;
        HighlightManager = new HighlightManager(HighlightCompiler.GetInstance().Compile(highlighter));
        Explanation = explanation;
    }
    
    public ChangeReport(string description, params Highlight[] highlighters)
    {
        Description = description;

        IHighlightable[] compiled = new IHighlightable[highlighters.Length];

        for (int i = 0; i < highlighters.Length; i++)
        {
            compiled[i] = HighlightCompiler.GetInstance().Compile(highlighters[i]);
        }
        
        HighlightManager = new HighlightManager(compiled);
        Explanation = null;
    }
    
    public ChangeReport(string description, Highlight first, params Highlight[] highlighters)
    {
        Description = description;

        IHighlightable[] compiled = new IHighlightable[highlighters.Length + 1];
        compiled[0] = HighlightCompiler.GetInstance().Compile(first);

        for (int i = 0; i < highlighters.Length; i++)
        {
            compiled[i + 1] = HighlightCompiler.GetInstance().Compile(highlighters[i]);
        }
        
        HighlightManager = new HighlightManager(compiled);
        Explanation = null;
    }

    public static ChangeReport Default(IReadOnlyList<SolverProgress> changes)
    {
        return new ChangeReport("",
            lighter => IChangeReportBuilder.HighlightChanges(lighter, changes));
    }
}