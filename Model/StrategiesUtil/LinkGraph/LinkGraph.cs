using System.Collections;
using System.Collections.Generic;

namespace Model.StrategiesUtil.LinkGraph;

public class LinkGraph<T> : IEnumerable<T> where T : ILinkGraphElement
{
    private readonly Dictionary<T, HashSet<T>>[] _links = { new(), new() };

    public void AddLink(T one, T two, LinkStrength strength, LinkType type = LinkType.BiDirectional)
    {
        var index = (int)strength - 1;
        if (!_links[index].TryAdd(one, new HashSet<T> { two })) _links[index][one].Add(two);
        if (type != LinkType.BiDirectional) return;
        if (!_links[index].TryAdd(two, new HashSet<T> { one })) _links[index][two].Add(one);
    }

    public IEnumerable<T> GetLinks(T from, LinkStrength strength)
    {
        return !_links[(int)strength - 1].TryGetValue(from, out var result) ? new HashSet<T>(0) : result;
    }

    public delegate bool Filter(T one, T two);
    
    public IEnumerable<T> GetLinks(T from, LinkStrength strength, Filter filter)
    {
        if (!_links[(int)strength - 1].TryGetValue(from, out var result)) yield break;
        foreach (var to in result)
        {
            if (filter(from, to)) yield return to;
        }
    }

    public bool HasLinkTo(T from, T to, LinkStrength strength)
    {
        return _links[(int)strength - 1].TryGetValue(from, out var r) && r.Contains(to);
    }

    public IEnumerable<T> EveryVerticesWith(LinkStrength strength)
    {
        return _links[(int)strength - 1].Keys;
    }

    public bool Contains(T vertex)
    {
        return _links[0].ContainsKey(vertex) || _links[1].ContainsKey(vertex);
    }

    public void Clear()
    {
        _links[0].Clear();
        _links[1].Clear();
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        HashSet<T> alreadyEnumerated = new();
        foreach (var value in _links[0].Keys)
        {
            yield return value;
            alreadyEnumerated.Add(value);
        }

        foreach (var value in _links[1].Keys)
        {
            if (!alreadyEnumerated.Contains(value)) yield return value;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public enum LinkStrength
{
    None = 0, Strong = 1, Weak = 2
}

public enum LinkType
{
    BiDirectional, MonoDirectional
}