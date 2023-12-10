using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Global.Enums;
using Model.Solver.Helpers.Highlighting;

namespace Model.Solver.StrategiesUtility.Graphs;

public class Chain<TElement, TLink> : IEnumerable<TElement> where TElement : notnull where TLink : notnull
{
    public TElement[] Elements { get; }
    public TLink[] Links { get; protected init; }

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
    protected readonly TLink _lastLink;
    
    public Loop(TElement[] elements, TLink[] links, TLink lastLink) : base(elements, links)
    {
        _lastLink = lastLink;
    }
    
    public Loop(TElement[] elements, TLink[] links) : base(elements, CutLast(links))
    {
        _lastLink = links[^1];
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

        if (_lastLink.Equals(link)) handler(Elements[0], Elements[^1]);
    }
    
    public void ForEachLink(LinkHandler handler)
    {
        for (int i = 0; i < Links.Length; i++)
        {
            handler(Elements[i], Elements[i + 1]);
        }

        handler(Elements[0], Elements[^1]);
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
    protected readonly List<TElement> _elements;
    protected readonly List<TLink> _links;

    public int Count => _elements.Count;

    public ChainBuilder(TElement start)
    {
        _elements = new List<TElement>();
        _links = new List<TLink>();
        _elements.Add(start);
    }

    protected ChainBuilder(List<TElement> elements, List<TLink> links)
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

    public virtual Chain<TElement, TLink> ToChain()
    {
        return new Chain<TElement, TLink>(_elements.ToArray(), _links.ToArray());
    }
    
    public virtual Loop<TElement, TLink> ToLoop(TLink lastLink)
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

public class LinkGraphChain<T> : Chain<T, LinkStrength> where T : ILinkGraphElement
{
    public bool IsMonoDirectional { get; init; } = false;
    
    public LinkGraphChain(T[] elements, LinkStrength[] links) : base(elements, links)
    {
    }
    
    public LinkGraphChain(T element) : base(element)
    {
    }

    public LinkGraphChain<T> AddInFront(LinkStrength link, T element)
    {
        var eBuffer = new T[Elements.Length + 1];
        var lBuffer = new LinkStrength[Links.Length + 1];

        Array.Copy(Elements, 0, eBuffer, 1, Elements.Length);
        Array.Copy(Links, 0, lBuffer, 1, Links.Length);
        eBuffer[0] = element;
        lBuffer[0] = link;

        return new LinkGraphChain<T>(eBuffer, lBuffer);
    }

    public LinkGraphLoop<T>? TryMakeLoop(LinkGraphChain<T> path)
    {
        if (IsMonoDirectional && path.IsMonoDirectional) return null;
        
        if (!path.Elements[0].Equals(Elements[0]) || !path.Elements[^1].Equals(Elements[^1])) return null;
        
        var total = Count + path.Count - 2;
        
        HashSet<T> all = new HashSet<T>(Elements);
        all.UnionWith(path.Elements);
        if (all.Count != total) return null;

        var elements = new T[total];
        var links = new LinkStrength[total];

        LinkGraphChain<T> first, second;
        switch (IsMonoDirectional, path.IsMonoDirectional)
        {
            case(false, false) :
            case(true, false) :
                first = this;
                second = path;
                break;
            case (false, true) :
                first = path;
                second = this;
                break;
            default : return null;
        }

        Array.Copy(first.Elements, 0, elements, 0, first.Elements.Length - 1);
        Array.Copy(second.Elements, 1, elements, first.Elements.Length - 1, second.Elements.Length - 1);
        Array.Reverse(elements, first.Elements.Length - 1, second.Elements.Length - 1);
            
        Array.Copy(first.Links, 0, links, 0, first.Links.Length);
        Array.Copy(second.Links, 0, links, first.Links.Length, second.Links.Length);
        Array.Reverse(links, first.Links.Length, second.Links.Length);

        return new LinkGraphLoop<T>(elements, links);
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
    
    public override string ToString()
    {
        var builder = new StringBuilder();
        for (int i = 0; i < Elements.Length - 1; i++)
        {
            builder.Append(Elements[i] + (Links[i] == LinkStrength.Strong ? " = " : " - "));
        }

        builder.Append(Elements[^1]);
        return builder.ToString();
    }
}

public class LinkGraphLoop<T> : Loop<T, LinkStrength> where T : ILinkGraphElement
{
    public LinkGraphLoop(T[] elements, LinkStrength[] links) : base(elements, links)
    {
    }
    
    public LinkGraphLoop(T[] elements, LinkStrength[] links, LinkStrength lastLink) : base(elements, links, lastLink)
    {
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
        if (obj is not LinkGraphLoop<T> loop) return false;
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
            if(i == Elements.Length - 1) result += Elements[i] + (_lastLink == LinkStrength.Strong ? " = " : " - ");
            else result += Elements[i] + (Links[i] == LinkStrength.Strong ? " = " : " - ");
        }

        return result;
    }
}

public class LinkGraphChainBuilder<T> : ChainBuilder<T, LinkStrength> where T : ILinkGraphElement
{
    public LinkGraphChainBuilder(T start) : base(start)
    {
    }

    private LinkGraphChainBuilder(List<T> elements, List<LinkStrength> links) : base(elements, links)
    {
        
    }
    
    public LinkGraphChainBuilder<T> Cut(int index)
    {
        var newElements = new List<T>();
        var newLinks = new List<LinkStrength>();

        for (int i = 0; i <= index; i++)
        {
            newElements[i] = _elements[i];
            if (i != index) newLinks[i] = _links[i];
        }
        
        return new LinkGraphChainBuilder<T>(newElements, newLinks);
    }
    
    public override LinkGraphChain<T> ToChain()
    {
        return new LinkGraphChain<T>(_elements.ToArray(), _links.ToArray());
    }
    
    public override LinkGraphLoop<T> ToLoop(LinkStrength lastLink)
    {
        return new LinkGraphLoop<T>(_elements.ToArray(), _links.ToArray(), lastLink);
    }
}