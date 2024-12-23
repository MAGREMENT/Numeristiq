﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Utility.Collections;

namespace Model.Core.Graphs;

public class Chain<TElement, TLink> : IEnumerable<TElement> where TElement : notnull where TLink : notnull
{
    public IReadOnlyList<TElement> Elements { get; }
    public IReadOnlyList<TLink> Links { get; }

    public Chain(IReadOnlyList<TElement> elements, IReadOnlyList<TLink> links) : this(elements, links, 1) {}

    public Chain(TElement element) : this(new[] { element }, Array.Empty<TLink>()) {}

    protected Chain(IReadOnlyList<TElement> elements, IReadOnlyList<TLink> links, int diff)
    {
        if (links.Count + diff != elements.Count) throw new ArgumentException("Incompatible element and link counts");
        Elements = elements;
        Links = links;
    }

    public int IndexOf(TElement element)
    {
        for (int i = 0; i < Elements.Count; i++)
        {
            if (element.Equals(Elements[i])) return i;
        }

        return -1;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Chain<TElement, TLink> chain || chain.Elements.Count != Elements.Count
                                                    || chain.Links.Count != Links.Count) return false;
        for (int i = 0; i < Elements.Count; i++)
        {
            if (!Elements[i].Equals(chain.Elements[i])) return false;
        }
        
        for (int i = 0; i < Links.Count; i++)
        {
            if (!Links[i].Equals(chain.Links[i])) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        int hash = 0;
        
        foreach (var e in Elements)
        {
            HashCode.Combine(e);
        }
        
        foreach (var l in Links)
        {
            HashCode.Combine(l);
        }

        return hash;
    }

    public override string ToString()
    {
        var builder = new StringBuilder(Elements[0].ToString());
        
        for (int i = 0; i < Elements.Count - 1; i++)
        {
            var l = Links[i] is LinkStrength ls ? ls.ToChar().ToString() : $"-{Links[i]}-";
            builder.Append($" {l} {Elements[i + 1]}");
        }

        if (Links.Count == Elements.Count)
        {
            var l = Links[^1] is LinkStrength ls ? ls.ToChar().ToString() : $"-{Links[^1]}-";
            builder.Append(" " + l);
        }

        return builder.ToString();
    }

    public IEnumerator<TElement> GetEnumerator()
    {
        return Elements.AsEnumerable().GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class Loop<TElement, TLink> : Chain<TElement, TLink> where TElement : notnull where TLink : notnull
{
    public Loop(IReadOnlyList<TElement> elements, IReadOnlyList<TLink> links) : base(elements, links, 0) {}
    
    public delegate void LinkHandler(TElement one, TElement two);

    public void ForEachLink(LinkHandler handler, TLink link)
    {
        for (int i = 0; i < Links.Count; i++)
        {
            if (Links[i].Equals(link)) handler(Elements[i], Elements[i < Elements.Count - 1 ? i + 1 : 0]);
        }
    }
    
    public void ForEachLink(LinkHandler handler)
    {
        for (int i = 0; i < Links.Count; i++)
        {
            handler(Elements[i], Elements[i < Elements.Count - 1 ? i + 1 : 0]);
        }
    }

    public bool Contains(TElement element)
    {
        var half = Elements.Count / 2;
        for (int i = 0; i < half; i++)
        {
            if (Elements[i].Equals(element)) return true;
            if (Elements[^(i + 1)].Equals(element)) return true;
        }

        if (Elements.Count % 2 == 1 && Elements[half].Equals(element)) return true;
        
        return false;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Loop<TElement, TLink> loop || loop.Elements.Count != Elements.Count) return false;

        var index = loop.IndexOf(Elements[0]);
        if (index == -1) return false;

        for (int i = 1; i < Elements.Count; i++)
        {
            if (!loop.Links[index].Equals(Links[i])) return false;
            
            index = (index + 1) % Elements.Count;
            if (!loop.Elements[index].Equals(Elements[i])) return false;
        }

        return loop.Links[index].Equals(Links[0]);
    }
}

public class ChainBuilder<TElement, TLink> where TElement : notnull where TLink : notnull
{
    private readonly List<TElement> _elements;
    private readonly List<TLink> _links;

    public int Count => _elements.Count;

    public ChainBuilder(TElement start)
    {
        _elements = new List<TElement>();
        _links = new List<TLink>();
        _elements.Add(start);
    }

    private ChainBuilder(List<TElement> elements, List<TLink> links)
    {
        _elements = elements;
        _links = links;
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
    
    public ChainBuilder<TElement, TLink> Cut(int index)
    {
        var newElements = new List<TElement>();
        var newLinks = new List<TLink>();

        for (int i = 0; i <= index; i++)
        {
            newElements[i] = _elements[i];
            if (i != index) newLinks[i] = _links[i];
        }
        
        return new ChainBuilder<TElement, TLink>(newElements, newLinks);
    }

    public Chain<TElement, TLink> ToChain()
    {
        return new Chain<TElement, TLink>(_elements.ToArray(), _links.ToArray());
    }
    
    public Loop<TElement, TLink> ToLoop(TLink lastLink)
    {
        _links.Add(lastLink);
        var buffer = _links.ToArray();
        _links.RemoveAt(_links.Count - 1);
        return new Loop<TElement, TLink>(_elements.ToArray(), buffer);
    }

    public TElement FirstElement()
    {
        return _elements[0];
    }

    public TElement LastElement()
    {
        return _elements[^1];
    }

    public TElement? BeforeLastElement()
    {
        if (_elements.Count < 2) return default;

        return _elements[^2];
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

    public bool ContainsElement(TElement element)
    {
        foreach (var e in _elements)
        {
            if (e.Equals(element)) return true;
        }

        return false;
    }
    
    public int IndexOf(TElement element)
    {
        for (int i = _elements.Count - 1; i >= 0; i--)
        {
            if (_elements[i].Equals(element)) return i;
        }

        return -1;
    }
}

public static class ChainExtensions
{
    public static Loop<T, LinkStrength>? TryMakeLoop<T>(this Chain<T, LinkStrength> path1, bool isMono1,
        Chain<T, LinkStrength> path2, bool isMono2) where T : notnull
    {
        if (isMono1 && isMono2) return null;
        
        if (!path2.Elements[0].Equals(path1.Elements[0]) || !path2.Elements[^1].Equals(path1.Elements[^1])) return null;
        
        var total = path1.Elements.Count + path2.Elements.Count - 2;
        
        var all = new HashSet<T>(path1.Elements);
        all.UnionWith(path2.Elements);
        if (all.Count != total) return null;

        var elements = new T[total];
        var links = new LinkStrength[total];

        Chain<T, LinkStrength> first, second;
        switch (isMono1, isMono2)
        {
            case(false, false) :
            case(true, false) :
                first = path1;
                second = path2;
                break;
            case (false, true) :
                first = path2;
                second = path1;
                break;
            default : return null;
        }

        first.Elements.CopyInto(0, elements, 0, first.Elements.Count - 1);
        second.Elements.CopyInto(1, elements, first.Elements.Count - 1, second.Elements.Count - 1);
        Array.Reverse(elements, first.Elements.Count - 1, second.Elements.Count - 1);
            
        first.Links.CopyInto(0, links, 0, first.Links.Count);
        second.Links.CopyInto(0, links, first.Links.Count, second.Links.Count);
        Array.Reverse(links, first.Links.Count, second.Links.Count);

        return new Loop<T, LinkStrength>(elements, links);
    }
    
    public static Chain<T, LinkStrength> ReconstructChain<T>(Dictionary<T, T> on,
        Dictionary<T, T> off, T startElement, LinkStrength firstLink, bool isLoop) where T : notnull
    {
        List<T> e = new();
        List<LinkStrength> l = new();

        e.Add(startElement);
        var link = firstLink;
        var current = on;
        while (current.TryGetValue(e[^1], out var friend))
        {
            e.Add(friend);
            l.Add(link);
            if (link == firstLink)
            {
                link = firstLink == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong;
                current = off;
            }
            else
            {
                link = firstLink;
                current = on;
            }
        }
        
        e.Reverse();
        l.Reverse();
        if (!isLoop) return new Chain<T, LinkStrength>(e, l);
        
        l.Add(firstLink);
        return new Loop<T, LinkStrength>(e, l);
    }
}