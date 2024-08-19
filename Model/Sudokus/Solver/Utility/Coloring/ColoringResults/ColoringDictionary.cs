using System.Collections.Generic;

namespace Model.Sudokus.Solver.Utility.Coloring.ColoringResults;

public class ColoringDictionary<T> : Dictionary<T, ElementColor>, IColoringResult<T> where T : notnull
{
    private ColoringHistory<T>? _history;

    public IReadOnlyColoringHistory<T>? History => _history;

    public void AddColoredElement(T element, ElementColor coloring)
    {
        TryAdd(element, coloring);
    }

    public void AddColoredElement(T element, ElementColor coloring, T parent)
    {
        AddColoredElement(element, coloring);
        _history?.Add(element, parent);
    }

    public bool TryGetColoredElement(T element, out ElementColor coloring)
    {
        return TryGetValue(element, out coloring);
    }

    public void NewStart()
    {
        
    }

    public void ActivateHistoryTracking()
    {
        _history = new ColoringHistory<T>();
    }
}