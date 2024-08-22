using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.Utility.Collections;

namespace Model.Core.Graphs.Implementations;

public abstract class DoubleDictionaryLinkGraph<T> : IGraph<T, LinkStrength> where T : notnull
{
    private readonly Dictionary<T, IContainingCollection<T>>[] _links =
    {
        new(), new()
    };

    protected abstract IContainingCollection<TCollectionType> CreateCollection<TCollectionType>() 
        where TCollectionType : notnull;

    public void Add(T from, T to, LinkStrength strength, LinkType type = LinkType.BiDirectional)
    {
        var dic = _links[(int)strength - 1];
        if (!dic.TryGetValue(from, out var collection))
        {
            collection = CreateCollection<T>();
            dic[from] = collection;
        }
        collection.Add(to);
        
        if (type != LinkType.BiDirectional) return;
        
        if (!dic.TryGetValue(to, out collection))
        {
            collection = CreateCollection<T>();
            dic[to] = collection;
        }
        collection.Add(from);
    }

    public virtual IEnumerable<T> Neighbors(T from, LinkStrength strength)
    {
        if (strength == LinkStrength.Any) return Neighbors(from);
        return _links[(int)strength - 1].TryGetValue(from, out var collection) 
            ? collection 
            : Enumerable.Empty<T>();
    }

    public virtual IEnumerable<T> Neighbors(T from)
    {
        if (_links[0].TryGetValue(from, out var collection))
        {
            foreach (var friend in collection)
            {
                yield return friend;
            }
        }
        
        if (_links[1].TryGetValue(from, out collection))
        {
            foreach (var friend in collection)
            {
                yield return friend;
            }
        }
    }

    public virtual IEnumerable<EdgeTo<LinkStrength, T>> NeighborsWithEdges(T from)
    {
        if (_links[0].TryGetValue(from, out var collection))
        {
            foreach (var friend in collection)
            {
                yield return new EdgeTo<LinkStrength, T>(LinkStrength.Strong, friend);
            }
        }
        
        if (_links[1].TryGetValue(from, out collection))
        {
            foreach (var friend in collection)
            {
                yield return new EdgeTo<LinkStrength, T>(LinkStrength.Weak, friend);
            }
        }
    }

    public virtual bool AreNeighbors(T from, T to, LinkStrength strength)
    {
        return _links[(int)strength - 1].TryGetValue(from, out var collection) && collection.Contains(to);
    }

    public virtual bool AreNeighbors(T from, T to)
    {
        return (_links[0].TryGetValue(from, out var collection) && collection.Contains(to)) ||
               (_links[1].TryGetValue(from, out collection) && collection.Contains(to));
    }

    public virtual LinkStrength LinkBetween(T from, T to)
    {
        if (_links[0].TryGetValue(from, out var collection) && collection.Contains(to)) return LinkStrength.Strong;
        if (_links[1].TryGetValue(from, out collection) && collection.Contains(to)) return LinkStrength.Weak;

        return LinkStrength.None;
    }

    public virtual void Clear()
    {
        _links[0].Clear();
        _links[1].Clear();
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        var set = new HashSet<T>();
        foreach (var e in _links[0].Keys)
        {
            yield return e;
            set.Add(e);
        }

        foreach (var e in _links[1].Keys)
        {
            if (!set.Contains(e)) yield return e;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class HDoubleDictionaryLinkGraph<T> : DoubleDictionaryLinkGraph<T> where T : notnull
{
    protected override IContainingCollection<TCollectionType> CreateCollection<TCollectionType>() 
        => new ContainingHashSet<TCollectionType>();
}

public class ULDoubleDictionaryLinkGraph<T> : DoubleDictionaryLinkGraph<T> where T : notnull
{
    protected override IContainingCollection<TCollectionType> CreateCollection<TCollectionType>() 
        => new UniqueList<TCollectionType>();
}