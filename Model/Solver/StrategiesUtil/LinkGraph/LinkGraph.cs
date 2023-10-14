using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Model.Solver.StrategiesUtil.LinkGraph;

public class LinkGraph<T> : IEnumerable<T> where T : notnull
{
    private readonly Dictionary<T, HashSet<T>[]> _links = new();

    public void AddLink(T from, T to, LinkStrength strength, LinkType type = LinkType.BiDirectional)
    {
        if (!_links.TryGetValue(from, out var resume))
        {
            resume = new[] { new HashSet<T>(), new HashSet<T>() };
            _links[from] = resume;
        }
        resume[(int)strength - 1].Add(to);
        
        if (type != LinkType.BiDirectional) return;
        
        
        if (!_links.TryGetValue(to, out resume))
        {
            resume = new[] { new HashSet<T>(), new HashSet<T>() };
            _links[to] = resume;
        }
        resume[(int)strength - 1].Add(from);
    }

    public IEnumerable<T> GetLinks(T from, LinkStrength strength)
    {
        return _links.TryGetValue(from, out var resume) ? resume[(int)strength - 1] : Enumerable.Empty<T>();
    }

    public bool HasLinkTo(T from, T to, LinkStrength strength)
    {
        return _links.TryGetValue(from, out var resume) && resume[(int)strength - 1].Contains(to);
    }

    public void Clear()
    {
        _links.Clear();
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        return _links.Keys.GetEnumerator();
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