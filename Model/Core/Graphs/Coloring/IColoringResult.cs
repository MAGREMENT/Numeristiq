namespace Model.Core.Graphs.Coloring;

public interface IColoringResult<T> : IValueCollection<T, ElementColor> where T : notnull
{
    public IReadOnlyColoringHistory<T>? History { get; }

    public void AddColoredElement(T element, ElementColor coloring);

    public void AddColoredElement(T element, ElementColor coloring, T parent);
    
    public bool TryGetColoredElement(T element, out ElementColor coloring);
    
    public void NewStart();

    public void ActivateHistoryTracking();

    bool IValueCollection<T, ElementColor>.TryGetElementValue(T element, out ElementColor value)
        => TryGetColoredElement(element, out value);
}