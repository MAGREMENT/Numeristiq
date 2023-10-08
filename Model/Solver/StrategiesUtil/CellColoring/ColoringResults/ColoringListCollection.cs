using System;
using System.Collections;
using System.Collections.Generic;

namespace Model.Solver.StrategiesUtil.CellColoring.ColoringResults;

public class ColoringListCollection<T> : IColoringResult<T>, IEnumerable<ColoringList<T>>
{
    private readonly List<ColoringList<T>> _lists = new();

    public void AddColoredElement(T vertex, Coloring coloring)
    {
        _lists[^1].Add(vertex, coloring);
    }

    public void NewStart()
    {
        _lists.Add(new ColoringList<T>());
    }

    public IEnumerator<ColoringList<T>> GetEnumerator()
    {
        return _lists.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class ColoringList<T>
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