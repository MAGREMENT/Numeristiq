using System.Collections.Generic;

namespace Model.Utility.Collections;

public interface IContainingEnumerable<T> : IEnumerable<T>
{
    bool Contains(T element);
}

public class ContainingList<T> : List<T>, IContainingEnumerable<T>
{
    
}

public class ContainingHashSet<T> : HashSet<T>, IContainingEnumerable<T>
{
    
}