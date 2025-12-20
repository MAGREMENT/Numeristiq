using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Model.Utility.Collections;

public class EqualizableArray<T> : IReadOnlyList<T>
{
    private readonly T[] _arr;

    public EqualizableArray(T[] arr)
    {
        _arr = arr;
    }

    public IEnumerator<T> GetEnumerator() => _arr.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _arr.GetEnumerator();

    public int Count => _arr.Length;

    public T this[int index] => _arr[index];

    public override bool Equals(object? obj)
    {
        if (obj is not EqualizableArray<T> other || other.Count != Count) return false;

        for (int i = 0; i < Count; i++)
        {
            if (this[i] is null)
            {
                if (other[i] != null) return false;
            }
            else if (!this[i]!.Equals(other[i])) return false;
        }
        
        return true;
    }

    public override int GetHashCode()
    {
        var code = new HashCode();

        foreach (var e in _arr)
        {
            code.Add(e);
        }
        
        return code.ToHashCode();
    }
}