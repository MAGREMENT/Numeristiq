using System;
using System.Collections.Generic;
using Global.Enums;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.StrategiesUtility.CellColoring;

public class ColoringHistory<T> : IReadOnlyColoringHistory<T> where T : ILinkGraphElement
{
    private readonly Dictionary<T, T> _parents = new();

    public void Add(T element, T parent)
    {
        _parents.TryAdd(element, parent);
    }

    public LinkGraphChain<T> GetPathToRootWithGuessedLinks(T from, Coloring coloring, bool reverse = true)
    {
        List<T> elements = new();
        List<LinkStrength> links = new();
        
        if (!_parents.TryGetValue(from, out var parent)) return new LinkGraphChain<T>(from);

        elements.Add(from);
        elements.Add(parent);
        links.Add(coloring == Coloring.On ? LinkStrength.Strong : LinkStrength.Weak);

        while (_parents.TryGetValue(parent, out var next))
        {
            elements.Add(next);
            parent = next;
            links.Add(links[^1] == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong);
        }

        var eArray = elements.ToArray();
        var lArray = links.ToArray();

        if (reverse)
        {
            Array.Reverse(eArray);
            Array.Reverse(lArray);
        }

        return new LinkGraphChain<T>(eArray, lArray);
    }
    
    public LinkGraphChain<T> GetPathToRootWithGuessedLinksAndMonoCheck(T from, Coloring coloring, LinkGraph<T> graph)
    {
        List<T> elements = new();
        List<LinkStrength> links = new();
        bool isMono = false;
        
        if (!_parents.TryGetValue(from, out var parent)) return new LinkGraphChain<T>(from);

        elements.Add(from);
        elements.Add(parent);
        links.Add(coloring == Coloring.On ? LinkStrength.Strong : LinkStrength.Weak);
        if (!graph.HasLinkTo(from, parent)) isMono = true;

        while (_parents.TryGetValue(parent, out var next))
        {
            elements.Add(next);
            parent = next;
            links.Add(links[^1] == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong);
            if (!graph.HasLinkTo(parent, next)) isMono = true;
        }

        var eArray = elements.ToArray();
        var lArray = links.ToArray();

        
        Array.Reverse(eArray);
        Array.Reverse(lArray);
        
        return new LinkGraphChain<T>(eArray, lArray)
        {
            IsMonoDirectional = isMono
        };
    }
    
    public LinkGraphChain<T> GetPathToRootWithRealLinks(T from, LinkGraph<T> graph, bool reverse = true)
    {
        List<T> elements = new();
        List<LinkStrength> links = new();
        
        if (!_parents.TryGetValue(from, out var parent)) return new LinkGraphChain<T>(from);

        elements.Add(from);
        elements.Add(parent);
        links.Add(graph.HasLinkTo(from, parent, LinkStrength.Strong) ? LinkStrength.Strong : LinkStrength.Weak);

        while (_parents.TryGetValue(parent, out var next))
        {
            elements.Add(next);
            links.Add(graph.HasLinkTo(parent, next, LinkStrength.Strong) ? LinkStrength.Strong : LinkStrength.Weak);
            parent = next;
        }

        var eArray = elements.ToArray();
        var lArray = links.ToArray();

        if (reverse)
        {
            Array.Reverse(eArray);
            Array.Reverse(lArray);
        }

        return new LinkGraphChain<T>(eArray, lArray);
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
    public LinkGraphChain<T> GetPathToRootWithGuessedLinks(T to, Coloring coloring, bool reverse = true);
    public LinkGraphChain<T> GetPathToRootWithRealLinks(T from, LinkGraph<T> graph, bool reverse = true);
    public LinkGraphChain<T> GetPathToRootWithGuessedLinksAndMonoCheck(T from, Coloring coloring, LinkGraph<T> graph);

    public void ForeachLink(HandleChildToParentLink<T> handler);
}

