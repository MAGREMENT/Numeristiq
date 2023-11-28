using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Global.Enums;

namespace Model.Solver.StrategiesUtility.Graphs;

public class Loop<T> : IEnumerable<T> where T : ILinkGraphElement
{
    public T[] Elements { get; }
    public LinkStrength[] Links { get; }

    public int Count => Elements.Length;

    public Loop(T[] elements, LinkStrength[] links)
    {
        Elements = elements;
        Links = links;
    }

    public delegate void LinkHandler(T one, T two);

    public void ForEachLink(LinkHandler handler, LinkStrength strength)
    {
        for (int i = 0; i < Links.Length - 1; i++)
        {
            if (Links[i] == strength) handler(Elements[i], Elements[i + 1]);
        }

        if (Links[^1] == strength) handler(Elements[0], Elements[^1]);
    }

    public int MaxRank()
    {
        var result = 0;
        foreach (var element in Elements)
        {
            result = Math.Max(result, element.Rank);
        }

        return result;
    }

    public override int GetHashCode()
    {
        int hash = 0;
        foreach (T element in Elements)
        {
            hash ^= EqualityComparer<T>.Default.GetHashCode(element);
        }

        return hash;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Loop<T> loop) return false;
        if (loop.Elements.Length != Elements.Length) return false;
        foreach (var element in Elements)
        {
            if (!loop.Elements.Contains(element)) return false;
        }

        return true;
    }

    public override string ToString()
    {
        string result = Elements[0] + (Links[0] == LinkStrength.Strong ? " = " : " - ");
        for (int i = 1; i < Elements.Length; i++)
        {
            result += Elements[i] + (Links[i] == LinkStrength.Strong ? " = " : " - ");
        }

        return result;
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        return Elements.AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class LoopBuilder<T> where T : ILinkGraphElement
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

    public LoopBuilder<T> Cut(int index)
    {
        T[] eBuffer = new T[_elements.Length - index];
        LinkStrength[] lBuffer = new LinkStrength[_links.Length - index];
        Array.Copy(_elements, index, eBuffer, 0, eBuffer.Length);
        Array.Copy(_links, index, lBuffer, 0, lBuffer.Length);
        
        return new LoopBuilder<T>(eBuffer, lBuffer);
    }

    public Loop<T> End(LinkStrength strength)
    {
        LinkStrength[] lBuffer = new LinkStrength[_links.Length + 1];
        Array.Copy(_links, lBuffer, _links.Length);
        lBuffer[_links.Length] = strength;

        return new Loop<T>(_elements, lBuffer);
    }

    public bool ContainsElement(T element)
    {
        foreach (var e in _elements)
        {
            if (e.Equals(element)) return true;
        }

        return false;
    }

    public int IndexOf(T element)
    {
        for (int i = _elements.Length - 1; i >= 0; i--)
        {
            if (_elements[i].Equals(element)) return i;
        }

        return -1;
    }

    public T? ElementBefore()
    {
        if (_elements.Length < 2) return default;
        return _elements[^2];
    }

    public T FirstElement()
    {
        return _elements[0];
    }
    
    public T LastElement()
    {
        return _elements[^1];
    }
    
    public LinkStrength FirstLink()
    {
        return _links.Length == 0 ? LinkStrength.None : _links[0];
    }

    public LinkStrength LastLink()
    {
        return _links.Length == 0 ? LinkStrength.None : _links[^1];
    }
    
    public Loop<T>? TryMerging(LoopBuilder<T> builder)
    {
        if (!LastElement().Equals(builder.LastElement()) || !FirstElement().Equals(builder.FirstElement())) return null;
        var fromFirst = new HashSet<T>(_elements);
        var elements = new List<T>(_elements);
        var links = new List<LinkStrength>(_links);

        for (int i = builder._elements.Length - 2; i > 0; i--)
        {
            var current = builder._elements[i];
            if (fromFirst.Contains(current)) return null;
            elements.Add(current);
            links.Add(builder._links[i]);
        }

        links.Add(builder._links[0]);

        return new Loop<T>(elements.ToArray(), links.ToArray());
    }

    public override string ToString()
    {
        string result = "";
        for (int i = 0; i < _elements.Length - 1; i++)
        {
            result += _elements[i] + (_links[i] == LinkStrength.Strong ? "=" : "-");
        }

        result += _elements[^1].ToString();

        return result;
    }

}