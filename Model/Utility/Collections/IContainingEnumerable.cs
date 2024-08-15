using System.Collections.Generic;

namespace Model.Utility.Collections;

public interface IContainingEnumerable<T> : IEnumerable<T>
{
    bool Contains(T element);
}

public interface IContainingCollection<T> : IContainingEnumerable<T>
{
    bool Add(T element);
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