using System;
using System.Collections.Generic;

namespace Model.Solver.StrategiesUtil.CellColoring.ColoringResults;

public class ColoringLists<T>
{
    private readonly List<T> _on = new();
    private readonly List<T> _off = new();

    public IReadOnlyList<T> On => _on;
    public IReadOnlyList<T> Off => _off;

    public int Count => On.Count + Off.Count;

    public void Add(T vertex, Coloring coloring)
    {
        switch (coloring)
        {
            case Coloring.On : _on.Add(vertex);
                break;
            case Coloring.Off : _off.Add(vertex);
                break;
            default: throw new ArgumentException("Invalid coloring");
        }
    }
}