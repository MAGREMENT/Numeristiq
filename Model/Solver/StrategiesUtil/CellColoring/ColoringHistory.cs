using System.Collections.Generic;

namespace Model.Solver.StrategiesUtil.CellColoring;

public class ColoringHistory<T> : IReadOnlyColoringHistory<T> where T : notnull
{
    private readonly Dictionary<T, T> _parents = new();

    public void Add(T element, T parent)
    {
        _parents.TryAdd(element, parent);
    }

    public List<T> GetPathToRoot(T from)
    {
        List<T> toReverse = new();
        if (!_parents.TryGetValue(from, out var parent)) return toReverse;

        toReverse.Add(from);
        toReverse.Add(parent);

        while (_parents.TryGetValue(parent, out var next))
        {
            toReverse.Add(next);
            parent = next;
        }

        List<T> result = new(toReverse.Count);
        for (int i = toReverse.Count - 1; i >= 0; i--)
        {
            result.Add(toReverse[i]);
        }

        return result;
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

public interface IReadOnlyColoringHistory<T>
{
    public List<T> GetPathToRoot(T from);

    public void ForeachLink(HandleChildToParentLink<T> handler);
}