using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;

namespace Model.Solver.Helpers.Logs;

public interface ISolverLog
{
    public int Id { get; }
    public string Title { get; }
    public Intensity Intensity { get; }
    public string Changes { get; }
    public string Explanation { get; }
    public SolverState StateBefore { get; }
    public SolverState StateAfter { get; }
    public HighlightManager HighlightManager { get; }

}

public enum Intensity
{
    Zero, One, Two, Three, Four, Five, Six
}