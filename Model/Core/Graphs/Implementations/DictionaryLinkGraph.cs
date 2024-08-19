using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.Utility.Collections;

namespace Model.Core.Graphs.Implementations;

public abstract class DictionaryLinkGraph<T> : IGraph<T, LinkStrength> where T : notnull
{
    private readonly Dictionary<T, IContainingCollection<T>[]> _links = new();

    protected abstract IContainingCollection<TCollectionType> CreateCollection<TCollectionType>() 
        where TCollectionType : notnull;

    public void Add(T from, T to, LinkStrength strength, LinkType type = LinkType.BiDirectional)
    {
        if (!_links.TryGetValue(from, out var collection))
        {
            collection = new[] { CreateCollection<T>(), CreateCollection<T>() };
            _links[from] = collection;
        }
        collection[(int)strength - 1].Add(to);
        
        if (type != LinkType.BiDirectional) return;
        
        if (!_links.TryGetValue(to, out collection))
        {
            collection = new[] { CreateCollection<T>(), CreateCollection<T>() };
            _links[to] = collection;
        }
        collection[(int)strength - 1].Add(from);
    }

    public virtual IEnumerable<T> Neighbors(T from, LinkStrength strength)
    {
        if (strength == LinkStrength.Any) return Neighbors(from);
        return _links.TryGetValue(from, out var resume) ? resume[(int)strength - 1] : Enumerable.Empty<T>();
    }

    public virtual IEnumerable<T> Neighbors(T from)
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

    public virtual IEnumerable<EdgeTo<LinkStrength, T>> NeighborsWithEdges(T from)
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

    public virtual bool AreNeighbors(T from, T to, LinkStrength strength)
    {
        return _links.TryGetValue(from, out var resume) && resume[(int)strength - 1].Contains(to);
    }

    public virtual bool AreNeighbors(T from, T to)
    {
        return _links.TryGetValue(from, out var resume) && (resume[0].Contains(to) ||
                                                            resume[1].Contains(to));
    }

    public virtual LinkStrength LinkBetween(T from, T to)
    {
        if (!_links.TryGetValue(from, out var sets))
        {
            return LinkStrength.None;
        }

        if (sets[0].Contains(to)) return LinkStrength.Strong;
        if (sets[1].Contains(to)) return LinkStrength.Weak;

        return LinkStrength.None;
    }

    public virtual void Clear()
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
    protected override IContainingCollection<TCollectionType> CreateCollection<TCollectionType>() 
        => new ContainingHashSet<TCollectionType>();
}

public class ULDictionaryLinkGraph<T> : DictionaryLinkGraph<T> where T : notnull
{
    protected override IContainingCollection<TCollectionType> CreateCollection<TCollectionType>() 
        => new UniqueList<TCollectionType>();
}

public abstract class DictionaryConditionalLinkGraph<TElement, TValue> : DictionaryLinkGraph<TElement>,
    IConditionalGraph<TElement, LinkStrength, TValue> where TElement : notnull
{
    public ValueCollection<TElement, TValue>? Values { get; set; }
    
    private readonly Dictionary<TElement, IContainingCollection<(TElement, ICondition<TElement, TValue>)>[]>
        _conditionalLinks = new();
    
    public void Add(ICondition<TElement, TValue> condition, TElement from, TElement to, LinkStrength edge,
        LinkType type = LinkType.BiDirectional)
    {
        if (!_conditionalLinks.TryGetValue(from, out var collection))
        {
            collection = new[] { CreateCollection<(TElement, ICondition<TElement, TValue>)>(),
                CreateCollection<(TElement, ICondition<TElement, TValue>)>() };
            _conditionalLinks[from] = collection;
        }
        collection[(int)edge - 1].Add((to, condition));
        
        if (type != LinkType.BiDirectional) return;
        
        if (!_conditionalLinks.TryGetValue(to, out collection))
        {
            collection = new[] { CreateCollection<(TElement, ICondition<TElement, TValue>)>(),
                CreateCollection<(TElement, ICondition<TElement, TValue>)>() };
            _conditionalLinks[to] = collection;
        }

        collection[(int)edge - 1].Add((from, condition));
    }

    public override IEnumerable<TElement> Neighbors(TElement from, LinkStrength strength)
    {
        var b = base.Neighbors(from, strength);
        return Values is null ? b : b.Concat(ConditionalNeighbors(from, strength));
    }

    private IEnumerable<TElement> ConditionalNeighbors(TElement from, LinkStrength strength)
    {
        if(!_conditionalLinks.TryGetValue(from, out var collection)) yield break;
        foreach (var (e, c) in collection[(int)strength - 1] )
        {
            if (c.IsMet(Values!)) yield return e;
        }
    }
    
    public override IEnumerable<TElement> Neighbors(TElement from)
    {
        var b = base.Neighbors(from);
        return Values is null ? b : b.Concat(ConditionalNeighbors(from));
    }
    
    private IEnumerable<TElement> ConditionalNeighbors(TElement from)
    {
        if(!_conditionalLinks.TryGetValue(from, out var collection)) yield break;
        foreach (var (e, c) in collection[0] )
        {
            if (c.IsMet(Values!)) yield return e;
        }
        
        foreach (var (e, c) in collection[1] )
        {
            if (c.IsMet(Values!)) yield return e;
        }
    }

    public override IEnumerable<EdgeTo<LinkStrength, TElement>> NeighborsWithEdges(TElement from)
    {
        var b = base.NeighborsWithEdges(from);
        return Values is null ? b : b.Concat(ConditionalNeighborsWithEdge(from));
    }
    
    private IEnumerable<EdgeTo<LinkStrength, TElement>> ConditionalNeighborsWithEdge(TElement from)
    {
        if(!_conditionalLinks.TryGetValue(from, out var collection)) yield break;
        foreach (var (e, c) in collection[0] )
        {
            if (c.IsMet(Values!)) yield return new EdgeTo<LinkStrength, TElement>(LinkStrength.Strong, e);
        }
        
        foreach (var (e, c) in collection[1] )
        {
            if (c.IsMet(Values!)) yield return new EdgeTo<LinkStrength, TElement>(LinkStrength.Weak, e);
        }
    }

    public override bool AreNeighbors(TElement from, TElement to, LinkStrength strength)
    {
        if (base.AreNeighbors(from, to, strength)) return true;
        if (Values is null || !_conditionalLinks.TryGetValue(from, out var collection)) return false;
        
        foreach (var (e, c) in collection[(int)strength - 1] )
        {
            if (e.Equals(to) && c.IsMet(Values)) return true;
        }

        return false;
    }
    
    public override bool AreNeighbors(TElement from, TElement to)
    {
        if (base.AreNeighbors(from, to)) return true;
        if (Values is null || !_conditionalLinks.TryGetValue(from, out var collection)) return false;
        
        foreach (var (e, c) in collection[0] )
        {
            if (e.Equals(to) && c.IsMet(Values)) return true;
        }
        
        foreach (var (e, c) in collection[1] )
        {
            if (e.Equals(to) && c.IsMet(Values)) return true;
        }

        return false;
    }

    public override LinkStrength LinkBetween(TElement from, TElement to)
    {
        var b = base.LinkBetween(from, to);
        if (b != LinkStrength.None) return b;

        if (Values is null || !_conditionalLinks.TryGetValue(from, out var collection)) return LinkStrength.None;
        
        foreach (var (e, c) in collection[0] )
        {
            if (e.Equals(to) && c.IsMet(Values)) return LinkStrength.Strong;
        }
        
        foreach (var (e, c) in collection[1] )
        {
            if (e.Equals(to) && c.IsMet(Values)) return LinkStrength.Weak;
        }

        return LinkStrength.None;
    }

    public override void Clear()
    {
        base.Clear();
        _conditionalLinks.Clear();
    }
}

public class HDictionaryConditionalLinkGraph<TElement, TValue> : DictionaryConditionalLinkGraph<TElement, TValue> where TElement : notnull
{
    protected override IContainingCollection<TCollectionType> CreateCollection<TCollectionType>() 
        => new ContainingHashSet<TCollectionType>();
}

public class ULDictionaryConditionalLinkGraph<TElement, TValue> : DictionaryConditionalLinkGraph<TElement, TValue> where TElement : notnull
{
    protected override IContainingCollection<TCollectionType> CreateCollection<TCollectionType>() 
        => new UniqueList<TCollectionType>();
}