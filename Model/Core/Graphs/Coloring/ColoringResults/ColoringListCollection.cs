using System;
using System.Collections;
using System.Collections.Generic;

namespace Model.Core.Graphs.Coloring.ColoringResults;

public class ColoringListCollection<T> : IColoringResult<T>, IEnumerable<ColoringList<T>> where T : notnull
{
    private bool _withHistory;
    
    public IReadOnlyColoringHistory<T>? History => _lists.Count == 0 ? null : _lists[^1].History;

    private readonly List<ColoringList<T>> _lists = new();

    public void AddColoredElement(T vertex, ElementColor coloring)
    {
        if (_lists.Count == 0) return;
        _lists[^1].Add(vertex, coloring);
    }

    public void AddColoredElement(T element, ElementColor coloring, T parent)
    {
        if (_lists.Count == 0) return;
        _lists[^1].Add(element, coloring, parent);
    }

    public bool TryGetColoredElement(T element, out ElementColor coloring)
    {
        foreach (var list in _lists)
        {
            foreach (var e in list.On)
            {
                if (element.Equals(e))
                {
                    coloring = ElementColor.On;
                    return true;
                }
            }

            foreach (var e in list.Off)
            {
                if (element.Equals(e))
                {
                    coloring = ElementColor.Off;
                    return true;
                }
            }
        }

        coloring = ElementColor.None;
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

public class ColoringList<T> where T : notnull
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

    public void Add(T vertex, ElementColor coloring)
    {
        switch (coloring)
        {
            case ElementColor.On : _on.Add(vertex);
                break;
            case ElementColor.Off : _off.Add(vertex);
                break;
            default: throw new ArgumentException("Invalid coloring");
        }
    }
    
    public void Add(T vertex, ElementColor coloring, T parent)
    {
        switch (coloring)
        {
            case ElementColor.On : _on.Add(vertex);
                break;
            case ElementColor.Off : _off.Add(vertex);
                break;
            default: throw new ArgumentException("Invalid coloring");
        }

        _history?.Add(vertex, parent);
    }
}