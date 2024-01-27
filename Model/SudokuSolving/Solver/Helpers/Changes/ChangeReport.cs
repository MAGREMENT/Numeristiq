using System.Collections.Generic;
using Model.SudokuSolving.Solver.Explanation;
using Model.SudokuSolving.Solver.Helpers.Highlighting;

namespace Model.SudokuSolving.Solver.Helpers.Changes;

public class ChangeReport
{
    public string Description { get; }
    public ExplanationElement? Explanation { get; }
    public HighlightManager HighlightManager { get; }
    public string Changes { get; }
    
    public ChangeReport(string changes, string description, Highlight highlighter)
    {
        Description = description;
        Changes = changes;
        HighlightManager = new HighlightManager(HighlightCompiler.GetInstance().Compile(highlighter));
        Explanation = null;
    }
    
    public ChangeReport(string changes, string description, Highlight highlighter, ExplanationElement? explanation)
    {
        Description = description;
        Changes = changes;
        HighlightManager = new HighlightManager(HighlightCompiler.GetInstance().Compile(highlighter));
        Explanation = explanation;
    }
    
    public ChangeReport(string changes, string description, params Highlight[] highlighters)
    {
        Description = description;
        Changes = changes;

        IHighlightable[] compiled = new IHighlightable[highlighters.Length];

        for (int i = 0; i < highlighters.Length; i++)
        {
            compiled[i] = HighlightCompiler.GetInstance().Compile(highlighters[i]);
        }
        
        HighlightManager = new HighlightManager(compiled);
        Explanation = null;
    }
    
    public ChangeReport(string changes, string description, Highlight first, params Highlight[] highlighters)
    {
        Description = description;
        Changes = changes;

        IHighlightable[] compiled = new IHighlightable[highlighters.Length + 1];
        compiled[0] = HighlightCompiler.GetInstance().Compile(first);

        for (int i = 0; i < highlighters.Length; i++)
        {
            compiled[i + 1] = HighlightCompiler.GetInstance().Compile(highlighters[i]);
        }
        
        HighlightManager = new HighlightManager(compiled);
        Explanation = null;
    }

    public static ChangeReport Default(IReadOnlyList<SolverChange> changes)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "",
            lighter => IChangeReportBuilder.HighlightChanges(lighter, changes));
    }
}