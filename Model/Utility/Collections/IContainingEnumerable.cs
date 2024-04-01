using System.Collections.Generic;

namespace Model.Utility.Collections;

public interface IContainingEnumerable<T> : IEnumerable<T>
{
    bool Contains(T element);
}