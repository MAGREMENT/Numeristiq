using System.Collections;
using System.Collections.Generic;

namespace Model.StrategiesUtil;

public class LinkGraph<T> : IEnumerable<T> where T : ILinkGraphElement
{
    private readonly Dictionary<T, HashSet<T>>[] _links = { new(), new() };

    public void AddLink(T one, T two, LinkStrength strength, LinkType type = LinkType.BiDirectional)
    {
        var index = (int)strength;
        if (!_links[index].TryAdd(one, new HashSet<T> { two })) _links[index][one].Add(two);
        if (type != LinkType.BiDirectional) return;
        if (!_links[index].TryAdd(two, new HashSet<T> { one })) _links[index][two].Add(one);
    }

    public HashSet<T> GetLinks(T from, LinkStrength strength)
    {
        return !_links[(int)strength].TryGetValue(from, out var result) ? new HashSet<T>(0) : result;
    }

    public bool IsOfStrength(T one, T two, LinkStrength strength)
    {
        return _links[(int)strength].TryGetValue(one, out var set) && set.Contains(two);
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

    public IEnumerable<T> EachVerticesWith(LinkStrength strength)
    {
        return _links[(int)strength].Keys;
    }
}

public enum LinkStrength
{
    None = -1, Strong = 0, Weak = 1
}

public enum LinkType
{
    BiDirectional, MonoDirectional
}