using Model.Sudoku.Solver.Explanation;
using Model.Sudoku.Solver.Helpers.Highlighting;

namespace Model.Sudoku.Solver.Helpers.Logs;

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

public enum Intensity
{
    Zero, One, Two, Three, Four, Five, Six
}