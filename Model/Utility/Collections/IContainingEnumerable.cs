using System.Collections.Generic;

namespace Model.Utility.Collections;

public interface IContainingEnumerable<T> : IEnumerable<T>
{
    bool Contains(T element);
}

public interface IContainingCollection<T> : IContainingEnumerable<T>
{
    int Count { get; }
    
    bool Add(T element);
    void Clear();
}

public class ContainingList<T> : List<T>, IContainingCollection<T>
{
    public new bool Add(T element)
    {
        base.Add(element);
        return true;
    }
}

public class ContainingHashSet<T> : HashSet<T>, IContainingCollection<T>
{
}