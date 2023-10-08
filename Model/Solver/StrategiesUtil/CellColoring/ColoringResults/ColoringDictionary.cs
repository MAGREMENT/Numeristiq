using System.Collections.Generic;

namespace Model.Solver.StrategiesUtil.CellColoring.ColoringResults;

public class ColoringDictionary<T> : Dictionary<T, Coloring>, IColoringResult<T> where T : notnull
{
    public void AddColoredElement(T element, Coloring coloring)
    {
        Add(element, coloring);
    }

    public bool TryGetColoredElement(T element, out Coloring coloring)
    {
        return TryGetValue(element, out coloring);
    }

    public void NewStart()
    {
        
    }
}