using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.StrategiesUtility.CellColoring;

public interface IColoringResult<T> where T : ISudokuElement
{
    public IReadOnlyColoringHistory<T>? History { get; }

    public void AddColoredElement(T element, Coloring coloring);

    public void AddColoredElement(T element, Coloring coloring, T parent);
    
    public bool TryGetColoredElement(T element, out Coloring coloring);
    
    public void NewStart();

    public void ActivateHistoryTracking();
}