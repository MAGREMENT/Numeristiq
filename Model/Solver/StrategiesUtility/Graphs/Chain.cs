using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Solver.StrategiesUtility.Graphs;

public class Chain<TElement, TLink> where TElement : notnull where TLink : notnull //TODO merge with Path & LinkGraphChain classes
{
    public TElement[] Elements { get; }
    public TLink[] Links { get; }

    public int Count => Elements.Length;

    public Chain(TElement[] elements, TLink[] links)
    {
        if (links.Length + 1 != elements.Length) throw new ArgumentException("Not a chain");
        Elements = elements;
        Links = links;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Chain<TElement, TLink> chain || chain.Count != Count) return false;
        for (int i = 0; i < Links.Length; i++)
        {
            if (!Elements[i].Equals(chain.Elements[i])) return false;
            if (!Links[i].Equals(chain.Links[i])) return false;
        }

        return Elements[^1].Equals(chain.Elements[^1]);
    }

    public override int GetHashCode()
    {
        int hash = 0;
        for (int i = 0; i < Links.Length; i++)
        {
            hash ^= Elements[i].GetHashCode() ^ Links[i].GetHashCode();
        }

        return hash ^ Elements[^1].GetHashCode();
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        for (int i = 0; i < Links.Length; i++)
        {
            builder.Append($"{Elements[i]} -{Links[i]}- ");
        }

        builder.Append(Elements[^1]);

        return builder.ToString();
    }
}

public class Loop<TElement, TLink> : Chain<TElement, TLink> where TElement : notnull where TLink : notnull
{
    private readonly TLink _lastLink;
    
    public Loop(TElement[] elements, TLink[] links, TLink lastLink) : base(elements, links)
    {
        _lastLink = lastLink;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Loop<TElement, TLink> loop) return false;
        return base.Equals(loop) && _lastLink.Equals(loop._lastLink);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode() ^ _lastLink.GetHashCode();
    }
}

public class ChainBuilder<TElement, TLink> where TElement : notnull where TLink : notnull
{
    private readonly List<TElement> _elements = new();
    private readonly List<TLink> _links = new();

    public int Count => _elements.Count;

    public ChainBuilder(TElement start)
    {
        _elements.Add(start);
    }

    public void Add(TLink link, TElement element)
    {
        _elements.Add(element);
        _links.Add(link);
    }

    public void RemoveLast()
    {
        if (_elements.Count == 1) return;
        
        _elements.RemoveAt(_elements.Count - 1);
        _links.RemoveAt(_elements.Count - 1);
    }

    public Chain<TElement, TLink> ToChain()
    {
        return new Chain<TElement, TLink>(_elements.ToArray(), _links.ToArray());
    }
    
    public Loop<TElement, TLink> ToLoop(TLink lastLink)
    {
        return new Loop<TElement, TLink>(_elements.ToArray(), _links.ToArray(), lastLink);
    }

    public TElement FirstElement()
    {
        return _elements[0];
    }

    public TElement LastElement()
    {
        return _elements[^1];
    }

    public TLink? FirstLink()
    {
        if (_links.Count == 0) return default;

        return _links[0];
    }

    public TLink? LastLink()
    {
        if (_links.Count == 0) return default;

        return _links[^1];
    }
}