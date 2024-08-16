using System;
using System.Collections.Generic;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;

namespace Model.Sudokus.Solver.Utility.CellColoring;

public class ColoringHistory<T> : IReadOnlyColoringHistory<T> where T : ISudokuElement
{
    private readonly Dictionary<T, T> _parents = new();

    public void Add(T element, T parent)
    {
        _parents.TryAdd(element, parent);
    }

    public bool ContainsChild(T child)
    {
        return _parents.ContainsKey(child);
    }

    public void RemoveChild(T child)
    {
        _parents.Remove(child);
    }

    public Chain<T, LinkStrength> GetPathToRootWithGuessedLinks(T from, Coloring coloring, bool reverse = true)
    {
        List<T> elements = new();
        List<LinkStrength> links = new();
        
        if (!_parents.TryGetValue(from, out var parent)) return new Chain<T, LinkStrength>(from);

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

        return new Chain<T, LinkStrength>(eArray, lArray);
    }
    
    public (Chain<T, LinkStrength>, bool) GetPathToRootWithGuessedLinksAndMonoCheck(T from, Coloring coloring, IGraph<T, LinkStrength> graph)
    {
        List<T> elements = new();
        List<LinkStrength> links = new();
        bool isMono = false;
        
        if (!_parents.TryGetValue(from, out var parent)) return (new Chain<T, LinkStrength>(from), isMono);

        elements.Add(from);
        elements.Add(parent);
        links.Add(coloring == Coloring.On ? LinkStrength.Strong : LinkStrength.Weak);
        if (!graph.AreNeighbors(from, parent)) isMono = true;

        while (_parents.TryGetValue(parent, out var next))
        {
            elements.Add(next);
            parent = next;
            links.Add(links[^1] == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong);
            if (!graph.AreNeighbors(parent, next)) isMono = true;
        }

        var eArray = elements.ToArray();
        var lArray = links.ToArray();

        
        Array.Reverse(eArray);
        Array.Reverse(lArray);

        return (new Chain<T, LinkStrength>(eArray, lArray), isMono);
    }
    
    public Chain<T, LinkStrength> GetPathToRootWithRealLinks(T from, IGraph<T, LinkStrength> graph, bool reverse = true)
    {
        List<T> elements = new();
        List<LinkStrength> links = new();
        
        if (!_parents.TryGetValue(from, out var parent)) return new Chain<T, LinkStrength>(from);

        elements.Add(from);
        elements.Add(parent);
        links.Add(graph.AreNeighbors(parent, from, LinkStrength.Strong) ? LinkStrength.Strong : LinkStrength.Weak);

        while (_parents.TryGetValue(parent, out var next))
        {
            elements.Add(next);
            links.Add(graph.AreNeighbors(next, parent, LinkStrength.Strong) ? LinkStrength.Strong : LinkStrength.Weak);
            parent = next;
        }

        var eArray = elements.ToArray();
        var lArray = links.ToArray();

        if (reverse)
        {
            Array.Reverse(eArray);
            Array.Reverse(lArray);
        }

        return new Chain<T, LinkStrength>(eArray, lArray);
    }
    
    public (Chain<T, LinkStrength>, bool) GetPathToRootWithRealLinksAndMonoCheck(T from, IGraph<T, LinkStrength> graph)
    {
        List<T> elements = new();
        List<LinkStrength> links = new();
        bool isMono = false;
        
        if (!_parents.TryGetValue(from, out var parent)) return (new Chain<T, LinkStrength>(from), isMono);

        elements.Add(from);
        elements.Add(parent);
        links.Add(graph.AreNeighbors(parent, from, LinkStrength.Strong) ? LinkStrength.Strong : LinkStrength.Weak);
        if (!graph.AreNeighbors(from, parent)) isMono = true;

        while (_parents.TryGetValue(parent, out var next))
        {
            elements.Add(next);
            links.Add(graph.AreNeighbors(next, parent, LinkStrength.Strong) ? LinkStrength.Strong : LinkStrength.Weak);
            parent = next;
            if (!graph.AreNeighbors(parent, next)) isMono = true;
        }

        var eArray = elements.ToArray();
        var lArray = links.ToArray();

        Array.Reverse(eArray);
        Array.Reverse(lArray);

        return (new Chain<T, LinkStrength>(eArray, lArray), isMono);
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

public interface IReadOnlyColoringHistory<T> where T : ISudokuElement
{
    public Chain<T, LinkStrength> GetPathToRootWithGuessedLinks(T to, Coloring coloring, bool reverse = true);
    public Chain<T, LinkStrength> GetPathToRootWithRealLinks(T from, IGraph<T, LinkStrength> graph, bool reverse = true);
    public (Chain<T, LinkStrength>, bool) GetPathToRootWithGuessedLinksAndMonoCheck(T from, Coloring coloring, IGraph<T, LinkStrength> graph);
    public (Chain<T, LinkStrength>, bool) GetPathToRootWithRealLinksAndMonoCheck(T from, IGraph<T, LinkStrength> graph);

    public void ForeachLink(HandleChildToParentLink<T> handler);
}

