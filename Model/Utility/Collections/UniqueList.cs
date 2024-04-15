using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Model.Utility.Collections;

public class UniqueList<T> : IReadOnlyList<T> where T : notnull
{
    private const int StartLength = 4;

    private T[] _array;
    public int Count { get; private set; }

    public UniqueList()
    {
        _array = Array.Empty<T>();
    }

    public UniqueList(int count)
    {
        _array = new T[count];
    }

    public UniqueList<T> Copy()
    {
        var list = new UniqueList<T>(Count)
        {
            Count = Count
        };
        Array.Copy(_array, list._array, Count);

        return list;
    }
    
    public int IndexOf(T obj) => Array.IndexOf(_array, obj, 0, Count);

    public bool Contains(T obj) => IndexOf(obj) != -1;

    public void Add(T obj, CallBackOnAlreadyPresent callback)
    {
        var i = IndexOf(obj);
        if (i != -1)
        {
            callback(i);
            return;
        }
        
        GrowIfNeeded();
        _array[Count] = obj;
        Count++;
    }

    public void Add(T obj)
    {
        var i = IndexOf(obj);
        if (i != -1) return;
        
        GrowIfNeeded();
        _array[Count] = obj;
        Count++;
    }

    public void AddRange(IEnumerable<T> objs)
    {
        foreach(var obj in objs) Add(obj);
    }

    public void Remove(T obj)
    {
        var i = IndexOf(obj);
        if (i != -1) RemoveAt(i);
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Count) return;
        
        if(index == Count - 1) _array[index] = default!;
        else
        {
            Array.Copy(_array, index + 1, _array, index, Count - index - 1);
        }

        Count--;
    }

    public void InsertAt(T obj, int index, CallBackOnAlreadyPresent callback)
    {
        if (index < 0 || index > Count) return;

        if (index == Count) Add(obj, callback);
        else
        {
            var i = IndexOf(obj);
            if (i != -1)
            {
                callback(i);
                return;
            }
            
            GrowIfNeeded();
            Array.Copy(_array, index, _array, index + 1, Count - index);
            _array[index] = obj;
            Count++;
        }
    }
    
    public void InsertAt(T obj, int index)
    {
        if (index < 0 || index > Count) return;

        if (index == Count) Add(obj);
        else
        {
            var i = IndexOf(obj);
            if (i != -1) return;
            
            GrowIfNeeded();
            Array.Copy(_array, index, _array, index + 1, Count - index);
            _array[index] = obj;
            Count++;
        }
    }

    public void Clear()
    {
        _array = Array.Empty<T>();
        Count = 0;
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return _array[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        if (Count == 0) return "";

        var builder = new StringBuilder(this[0].ToString());

        for (int i = 1; i < Count; i++)
        {
            builder.Append($", {this[i]}");
        }

        return builder.ToString();
    }

    public T this[int index] => _array[index];
    
    //Private-----------------------------------------------------------------------------------------------------------
    
    private void GrowIfNeeded()
    {
        if (Count < _array.Length) return;

        if (_array.Length == 0)
        {
            _array = new T[StartLength];
            return;
        }

        var buffer = new T[_array.Length * 2];
        Array.Copy(_array, 0, buffer, 0, _array.Length);
        _array = buffer;
    }
}

public delegate void CallBackOnAlreadyPresent(int index);
