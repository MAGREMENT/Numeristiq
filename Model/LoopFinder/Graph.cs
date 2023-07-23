using System.Collections;
using System.Collections.Generic;

namespace Model.LoopFinder;

public class Graph<T> : IEnumerable<T> where T : notnull
{
    private readonly HashSet<T> _vertices = new();
    private readonly Dictionary<T, HashSet<T>>[] _links = { new(), new() };

    public void AddLink(T one, T two, LinkStrength strength)
    {
        _vertices.Add(one);
        _vertices.Add(two);

        int index = (int)strength;
        if (!_links[index].TryAdd(one, new HashSet<T> { two })) _links[index][one].Add(two);
        if (!_links[index].TryAdd(two, new HashSet<T> { one })) _links[index][two].Add(one);
    }

    public HashSet<T> GetLinks(T from, LinkStrength strength)
    {
        if (!_links[(int)strength].TryGetValue(from, out var result)) return new HashSet<T>();
        return result;
    }

    public bool IsOfStrength(T one, T two, LinkStrength strength)
    {
        if (!_links[(int)strength].TryGetValue(one, out var set)) return false;
        return set.Contains(two);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _vertices.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public enum LinkStrength
{
    None = -1, Strong = 0, Weak = 1
}