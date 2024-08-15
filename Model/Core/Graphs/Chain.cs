using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model.Core.Graphs;

public class Chain<TElement, TLink> : IEnumerable<TElement> where TElement : notnull where TLink : notnull
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

    public Chain(TElement element)
    {
        Elements = new[] { element };
        Links = Array.Empty<TLink>();
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
    public TLink LastLink { get; }
    
    public Loop(TElement[] elements, TLink[] links, TLink lastLink) : base(elements, links)
    {
        LastLink = lastLink;
    }
    
    public Loop(TElement[] elements, TLink[] links) : base(elements, CutLast(links))
    {
        LastLink = links[^1];
    }

    private static TLink[] CutLast(TLink[] links)
    {
        var buffer = new TLink[links.Length - 1];
        Array.Copy(links, 0, buffer, 0, links.Length - 1);
        return buffer;
    }
    
    public delegate void LinkHandler(TElement one, TElement two);

    public void ForEachLink(LinkHandler handler, TLink link)
    {
        for (int i = 0; i < Links.Length; i++)
        {
            if (Links[i].Equals(link)) handler(Elements[i], Elements[i + 1]);
        }

        if (LastLink.Equals(link)) handler(Elements[0], Elements[^1]);
    }
    
    public void ForEachLink(LinkHandler handler)
    {
        for (int i = 0; i < Links.Length; i++)
        {
            handler(Elements[i], Elements[i + 1]);
        }

        handler(Elements[0], Elements[^1]);
    }

    public bool Contains(TElement element)
    {
        var half = Elements.Length / 2;
        for (int i = 0; i < half; i++)
        {
            if (Elements[i].Equals(element)) return true;
            if (Elements[^(i + 1)].Equals(element)) return true;
        }

        if (Elements.Length % 2 == 1 && Elements[half + 1].Equals(element)) return true;
        
        return false;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Loop<TElement, TLink> loop) return false;
        return base.Equals(loop) && LastLink.Equals(loop.LastLink);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode() ^ LastLink.GetHashCode();
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