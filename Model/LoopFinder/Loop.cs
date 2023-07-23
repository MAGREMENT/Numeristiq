using System;
using System.Collections.Generic;
using System.Linq;

namespace Model.LoopFinder;

public class Loop<T> where T : notnull
{
    private readonly T[] _elements;
    private readonly LinkStrength[] _links;

    public int Count => _elements.Length;

    public Loop(T[] elements, LinkStrength[] links)
    {
        _elements = elements;
        _links = links;
    }

    public override int GetHashCode()
    {
        int hash = 0;
        foreach (T element in _elements)
        {
            hash ^= EqualityComparer<T>.Default.GetHashCode(element);
        }
        return hash;
    }

    public Loop<T> Merge(Loop<T> other)
    {
        LoopBuilder<T> final = new(_elements[^1]);
        int otherIndex = -1;
        for (int i = _elements.Length - 2; i >= 0 && otherIndex != -1; i--)
        {
            final = final.Add(_elements[i], _links[i + 1]);

            for (int j = 0; j < other._elements.Length; j++)
            {
                if (other._elements[j].Equals(_elements[i])) otherIndex = j;
            }
        }

        return final.End(LinkStrength.None); //TODO
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Loop<T> loop) return false;
        if (loop._elements.Length != _elements.Length) return false;
        foreach (var element in _elements)
        {
            if (!loop._elements.Contains(element)) return false;
        }

        return true;
    }

    public override string ToString()
    {
        string result = _elements[0] + (_links[0] == LinkStrength.Strong ? "=" : "-");
        for (int i = 1; i < _elements.Length; i++)
        {
            result += _elements[i] + (_links[i] == LinkStrength.Strong ? "=" : "-");
        }

        return result;
    }

    public delegate void LinkHandler(T one, T two);

    public void ForEachLink(LinkHandler handler, LinkStrength strength)
    {
        for (int i = 0; i < _links.Length - 1; i++)
        {
            if (_links[i] == strength) handler(_elements[i], _elements[i + 1]);
        }

        if (_links[^1] == strength) handler(_elements[0], _elements[^1]);
    }

    public LinkStrength FindDoubleLink(out T value)
    {
        if (_links[0] == _links[^1])
        {
            value = _elements[0];
            return _links[0];
        }

        for (int i = 0; i < _links.Length; i++)
        {
            if (_links[i] == _links[i + 1])
            {
                value = _elements[i];
                return _links[i];
            }
        }

        value = default!;
        return LinkStrength.None;
    }
}

public class LoopBuilder<T> where T : notnull
{

    private readonly T[] _elements;
    private readonly LinkStrength[] _links;

    public int Count => _elements.Length;

    public LoopBuilder(T first)
    {
        _elements = new[] { first };
        _links = Array.Empty<LinkStrength>();
    }

    private LoopBuilder(T[] elements, LinkStrength[] links)
    {
        _elements = elements;
        _links = links;
    }

    public LoopBuilder<T> Add(T to, LinkStrength strength)
    {
        T[] eBuffer = new T[_elements.Length + 1];
        LinkStrength[] lBuffer = new LinkStrength[_links.Length + 1];
        Array.Copy(_elements, eBuffer, _elements.Length);
        Array.Copy(_links, lBuffer, _links.Length);
        eBuffer[_elements.Length] = to;
        lBuffer[_links.Length] = strength;

        return new LoopBuilder<T>(eBuffer, lBuffer);
    }

    public Loop<T> End(LinkStrength strength)
    {
        LinkStrength[] lBuffer = new LinkStrength[_links.Length + 1];
        Array.Copy(_links, lBuffer, _links.Length);
        lBuffer[_links.Length] = strength;

        return new Loop<T>(_elements, lBuffer);
    }

    public ContainedStatus Contains(T element)
    {
        if (_elements[0].Equals(element)) return ContainedStatus.First;
        for (int i = 1; i < _elements.Length; i++)
        {
            if (_elements[i].Equals(element)) return ContainedStatus.Contained;
        }

        return ContainedStatus.NotContained;
    }

    public T LastElement()
    {
        return _elements[^1];
    }

    public LinkStrength LastLink()
    {
        return _links.Length == 0 ? LinkStrength.None : _links[^1];
    }

    public LinkStrength FirstLink()
    {
        return _links.Length == 0 ? LinkStrength.None : _links[0];
    }

}

public enum ContainedStatus
{
    First, Contained, NotContained
}