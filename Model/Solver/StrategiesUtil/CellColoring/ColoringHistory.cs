using System;
using System.Collections.Generic;
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
}