using System.Collections.Generic;

namespace Model.Utility;

public interface IContainingEnumerable<T> : IEnumerable<T>
{
    bool Contains(T element);
}