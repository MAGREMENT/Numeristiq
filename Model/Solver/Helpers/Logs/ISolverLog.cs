using Global;
using Global.Enums;
using Model.Solver.Explanation;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;

namespace Model.Solver.Helpers.Logs;

public interface ISolverLog
{
    public int Id { get; }
    public string Title { get; }
    public Intensity Intensity { get; }
    public string Changes { get; }
    public string Description { get; }
    public ExplanationElement? Explanation { get; }
    public SolverState StateBefore { get; }
    public SolverState StateAfter { get; }
    public HighlightManager HighlightManager { get; }
    public bool FromSolving { get; }

}