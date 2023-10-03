using System.Collections.Generic;
using Model.Solver.Helpers.Highlighting;

namespace Model.Solver.Helpers.Changes;

public class ChangeReport
{
    public string Explanation { get; }
    public HighlightManager HighlightManager { get; }
    public string Changes { get; }
    
    public ChangeReport(string changes,  string explanation, Highlight highlighter)
    {
        Explanation = explanation;
        Changes = changes;
        HighlightManager = new HighlightManager(HighlightCompiler.GetInstance().Compile(highlighter));
    }
    
    public ChangeReport(string changes, string explanation, params Highlight[] highlighters)
    {
        Explanation = explanation;
        Changes = changes;

        IHighlighter[] compiled = new IHighlighter[highlighters.Length];

        for (int i = 0; i < highlighters.Length; i++)
        {
            compiled[i] = HighlightCompiler.GetInstance().Compile(highlighters[i]);
        }
        
        HighlightManager = new HighlightManager(compiled);
    }

    public static ChangeReport Default(List<SolverChange> changes)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "",
            lighter => IChangeReportBuilder.HighlightChanges(lighter, changes));
    }
}