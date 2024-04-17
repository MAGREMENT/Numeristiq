using System.Collections.Generic;
using Model.Sudokus.Solver.StrategiesUtility.Graphs;

namespace Model.Sudokus.Solver.StrategiesUtility.CellColoring.ColoringResults;

public class ColoringDictionary<T> : Dictionary<T, Coloring>, IColoringResult<T> where T : ISudokuElement
{
    private ColoringHistory<T>? _history;

    public IReadOnlyColoringHistory<T>? History => _history;

    public void AddColoredElement(T element, Coloring coloring)
    {
        TryAdd(element, coloring);
    }

    public void AddColoredElement(T element, Coloring coloring, T parent)
    {
        AddColoredElement(element, coloring);
        _history?.Add(element, parent);
    }

    public bool TryGetColoredElement(T element, out Coloring coloring)
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