using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.Utility.Collections;

namespace Model.Core.Graphs.Implementations;

public abstract class DictionaryLinkGraph<T> : IGraph<T, LinkStrength> where T : notnull
{
    private readonly Dictionary<T, IContainingCollection<T>[]> _links = new();

    protected abstract IContainingCollection<T> CreateCollection();

    public void Add(T from, T to, LinkStrength strength, LinkType type = LinkType.BiDirectional)
    {
        if (!_links.TryGetValue(from, out var resume))
        {
            resume = new[] { CreateCollection(), CreateCollection() };
            _links[from] = resume;
        }
        resume[(int)strength - 1].Add(to);
        
        if (type != LinkType.BiDirectional) return;
        
        if (!_links.TryGetValue(to, out resume))
        {
            resume = new[] { CreateCollection(), CreateCollection() };
            _links[to] = resume;
        }
        resume[(int)strength - 1].Add(from);
    }

    public IEnumerable<T> Neighbors(T from, LinkStrength strength)
    {
        if (strength == LinkStrength.Any) return Neighbors(from);
        return _links.TryGetValue(from, out var resume) ? resume[(int)strength - 1] : Enumerable.Empty<T>();
    }

    public IEnumerable<T> Neighbors(T from)
    {
        if (!_links.TryGetValue(from, out var resume)) yield break;
        
        foreach (var friend in resume[0])
        {
            yield return friend;
        }
            
        foreach (var friend in resume[1])
        {
            yield return friend;
        }
    }

    public IEnumerable<EdgeTo<LinkStrength, T>> NeighborsWithEdges(T from)
    {
        if (!_links.TryGetValue(from, out var resume)) yield break;
        
        foreach (var friend in resume[0])
        {
            yield return new EdgeTo<LinkStrength, T>(LinkStrength.Strong, friend);
        }
            
        foreach (var friend in resume[1])
        {
            yield return new EdgeTo<LinkStrength, T>(LinkStrength.Weak, friend);
        }
    }

    public bool AreNeighbors(T from, T to, LinkStrength strength)
    {
        return _links.TryGetValue(from, out var resume) && resume[(int)strength - 1].Contains(to);
    }

    public bool AreNeighbors(T from, T to)
    {
        return _links.TryGetValue(from, out var resume) && (resume[0].Contains(to) ||
                                                            resume[1].Contains(to));
    }

    public LinkStrength LinkBetween(T from, T to)
    {
        if (!_links.TryGetValue(from, out var sets))
        {
            return LinkStrength.None;
        }

        if (sets[0].Contains(to)) return LinkStrength.Strong;
        if (sets[1].Contains(to)) return LinkStrength.Weak;

        return LinkStrength.None;
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

public class HDictionaryLinkGraph<T> : DictionaryLinkGraph<T> where T : notnull
{
    protected override IContainingCollection<T> CreateCollection() => new ContainingHashSet<T>();
}

public class ULDictionaryLinkGraph<T> : DictionaryLinkGraph<T> where T : notnull
{
    protected override IContainingCollection<T> CreateCollection() => new UniqueList<T>();
}