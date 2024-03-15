using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Explanation;

namespace Model.Helpers.Logs;

public interface ISolverLog
{
    public int Id { get; }
    public string Title { get; }
    public Intensity Intensity { get; }
    public string Description { get; }
    public ExplanationElement? Explanation { get; }
    public string GetCursorPosition();
}

public interface ISolverLog<THighlighter> : ISolverLog
{ 
    public IReadOnlyList<SolverProgress> Changes { get; }
    public IUpdatableSolvingState StateBefore { get; }
    public IUpdatableSolvingState StateAfter { get; }
    public HighlightManager<THighlighter> HighlightManager { get; }
    public bool FromSolving { get; }
    string ISolverLog.GetCursorPosition()
    {
        return HighlightManager.CursorPosition();
    }
}

public enum Intensity
{
    Zero, One, Two, Three, Four, Five, Six
}