using Model.SudokuSolving.Solver.StrategiesUtility.Graphs;

namespace Model.SudokuSolving.Solver.StrategiesUtility.CellColoring;

public interface IColoringResult<T> where T : ISudokuElement
{
    public IReadOnlyColoringHistory<T>? History { get; }

    public void AddColoredElement(T element, Coloring coloring);

    public void AddColoredElement(T element, Coloring coloring, T parent);
    
    public bool TryGetColoredElement(T element, out Coloring coloring);
    
    public void NewStart();

    public void ActivateHistoryTracking();
}