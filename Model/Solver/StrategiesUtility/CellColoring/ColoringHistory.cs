using System.Collections.Generic;
using Global.Enums;
using Model.Solver.StrategiesUtility.LinkGraph;

namespace Model.Solver.StrategiesUtility.CellColoring;

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
        
        if (!_parents.TryGetValue(from, out var parent)) return new Path<T>(from);

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

