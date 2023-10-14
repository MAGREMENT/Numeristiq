using System;
using System.Collections.Generic;
using System.Text;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.StrategiesUtil.CellColoring;

public class ColoringHistory<T> : IReadOnlyColoringHistory<T> where T : ILinkGraphElement
{
    private readonly Dictionary<T, T> _parents = new();

    public void Add(T element, T parent)
    {
        _parents.TryAdd(element, parent);
    }

    public Path<T> GetPathToRoot(T from, Coloring coloring)
    {
        List<T> elementsToReverse = new();
        List<LinkStrength> linksToReverse = new();
        
        if (!_parents.TryGetValue(from, out var parent)) return new Path<T>();

        elementsToReverse.Add(from);
        elementsToReverse.Add(parent);
        linksToReverse.Add(coloring == Coloring.On ? LinkStrength.Strong : LinkStrength.Weak);

        while (_parents.TryGetValue(parent, out var next))
        {
            elementsToReverse.Add(next);
            parent = next;
            linksToReverse.Add(linksToReverse[^1] == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong);
        }

        List<T> elementsResult = new(elementsToReverse.Count);
        List<LinkStrength> linksResult = new(linksToReverse.Count);
        for (int i = elementsToReverse.Count - 1; i >= 0; i--)
        {
            elementsResult.Add(elementsToReverse[i]);
            if (i < linksToReverse.Count) linksResult.Add(linksToReverse[i]);
        }

        return new Path<T>(elementsResult.ToArray(), linksResult.ToArray());
    }

    public void ForeachLink(HandleChildToParentLink<T> handler)
    {
        foreach (var entry in _parents)
        {
            handler(entry.Key, entry.Value);
        }
    }
}

public delegate void HandleChildToParentLink<in T>(T child, T parent);

public interface IReadOnlyColoringHistory<T> where T : ILinkGraphElement
{
    public Path<T> GetPathToRoot(T from, Coloring coloring);

    public void ForeachLink(HandleChildToParentLink<T> handler);
}

public class Path<T> where T : ILinkGraphElement
{
    public Path(T[] elements, LinkStrength[] links)
    {
        if (elements.Length != links.Length + 1) throw new ArgumentException();
        Elements = elements;
        Links = links;
    }

    public Path()
    {
        Elements = Array.Empty<T>();
        Links = Array.Empty<LinkStrength>();
    }

    public T[] Elements { get; }
    public LinkStrength[] Links { get; }

    public int Count => Elements.Length;

    public void Highlight(IHighlightable highlighter)
    {
        for (int i = 0; i < Links.Length; i++)
        {
            var current = Links[i];
            highlighter.HighlightLinkGraphElement(Elements[i + 1], current == LinkStrength.Strong
                ? ChangeColoration.CauseOnOne : ChangeColoration.CauseOffOne);
            highlighter.CreateLink(Elements[i], Elements[i + 1], current);
        }
        
        if(Elements.Length > 0) highlighter.HighlightLinkGraphElement(Elements[0], Links[0] == LinkStrength.Strong
            ? ChangeColoration.CauseOffOne : ChangeColoration.CauseOnOne);
    }

    public Loop<T>? TryMakeLoop(Path<T> path)
    {
        if (!path.Elements[0].Equals(Elements[0]) || !path.Elements[^1].Equals(Elements[^1])) return null;
        HashSet<T> present = new HashSet<T>(Elements);

        var total = Count + path.Count - 2;
        
        var elements = new T[total];
        var links = new LinkStrength[total];

        Array.Copy(Elements, 0, elements, 0, Elements.Length);
        var cursor = Elements.Length;
        for (int i = path.Elements.Length - 2; i > 0; i--)
        {
            var current = path.Elements[i];
            if (present.Contains(current)) return null;

            elements[cursor++] = current;
        }

        Array.Copy(Links, 0, links, 0, Links.Length);
        cursor = Links.Length;
        for (int i = path.Links.Length - 1; i >= 0; i--)
        {
            links[cursor++] = path.Links[i];
        }

        return new Loop<T>(elements, links);
    }
    
    public override string ToString()
    {
        var builder = new StringBuilder();
        for (int i = 0; i < Elements.Length; i++)
        {
            builder.Append(Elements[i] + (Links[i] == LinkStrength.Strong ? " = " : " - "));
        }

        builder.Append(Elements[^1]);
        return builder.ToString();
    }
}