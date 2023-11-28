using System;
using System.Collections;
using System.Collections.Generic;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.StrategiesUtility.CellColoring.ColoringResults;

public class ColoringListCollection<T> : IColoringResult<T>, IEnumerable<ColoringList<T>> where T : ILinkGraphElement
{
    private bool _withHistory;
    
    public IReadOnlyColoringHistory<T>? History => _lists.Count == 0 ? null : _lists[^1].History;

    private readonly List<ColoringList<T>> _lists = new();

    public void AddColoredElement(T vertex, Coloring coloring)
    {
        if (_lists.Count == 0) return;
        _lists[^1].Add(vertex, coloring);
    }

    public void AddColoredElement(T element, Coloring coloring, T parent)
    {
        if (_lists.Count == 0) return;
        _lists[^1].Add(element, coloring, parent);
    }

    public bool TryGetColoredElement(T element, out Coloring coloring)
    {
        foreach (var list in _lists)
        {
            foreach (var e in list.On)
            {
                if (element.Equals(e))
                {
                    coloring = Coloring.On;
                    return true;
                }
            }

            foreach (var e in list.Off)
            {
                if (element.Equals(e))
                {
                    coloring = Coloring.Off;
                    return true;
                }
            }
        }

        coloring = Coloring.None;
        return false;
    }

    public void NewStart()
    {
        _lists.Add(new ColoringList<T>(_withHistory));
    }

    public void ActivateHistoryTracking()
    {
        _withHistory = true;
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

public class ColoringList<T> where T : ILinkGraphElement
{
    private readonly List<T> _on = new();
    private readonly List<T> _off = new();
    private readonly ColoringHistory<T>? _history;

    public IReadOnlyList<T> On => _on;
    public IReadOnlyList<T> Off => _off;
    public IReadOnlyColoringHistory<T>? History => _history;

    public int Count => On.Count + Off.Count;
    
    public ColoringList(bool withHistory)
    {
        _history = withHistory ? new ColoringHistory<T>() : null;
    }

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
    
    public void Add(T vertex, Coloring coloring, T parent)
    {
        switch (coloring)
        {
            case Coloring.On : _on.Add(vertex);
                break;
            case Coloring.Off : _off.Add(vertex);
                break;
            default: throw new ArgumentException("Invalid coloring");
        }

        _history?.Add(vertex, parent);
    }
}