namespace Model.Sudokus.Solver.Utility.Coloring;

public interface IColoringResult<T> where T : notnull
{
    public IReadOnlyColoringHistory<T>? History { get; }

    public void AddColoredElement(T element, ElementColor coloring);

    public void AddColoredElement(T element, ElementColor coloring, T parent);
    
    public bool TryGetColoredElement(T element, out ElementColor coloring);
    
    public void NewStart();

    public void ActivateHistoryTracking();
}