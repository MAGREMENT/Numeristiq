using System;
using System.Collections;
using System.Collections.Generic;

namespace Model.Utility;

public class NotifyingList<T> : IList, IList<T>
{
    private T[] _array = Array.Empty<T>();
    
    public int Count { get; private set; }
    public bool IsSynchronized => false;
    public object SyncRoot => null!;
    public bool IsFixedSize => false;
    public bool IsReadOnly => false;
    
    public event OnClear? Cleared;
    public event OnElementAdded<T>? ElementAdded; 
    
    public int Add(object? value)
    {
        if (value is not T item) return -1;

        Add(item);
        
        return Count - 1;
    }

    public void Add(T item)
    {
        GrowIfNecessary();

        _array[Count++] = item;
        ElementAdded?.Invoke(item);
    }

    public void Clear()
    {
        Count = 0;
        Cleared?.Invoke();
    }

    public bool Contains(T item)
    {
        for(int i = 0; i < Count; i++)
        {
            var o = _array[i];
            if (o is not null && o.Equals(item)) return true;
        }

        return false;
    }
    
    public bool Contains(object? value)
    {
        for(int i = 0; i < Count; i++)
        {
            var o = _array[i];
            if (o is null)
            {
                if (value is null) return true;
            }else if (o.Equals(value)) return true;
        }

        return false;
    }
    
    public void CopyTo(Array array, int index)
    {
        Array.Copy(_array, 0, array, index, Count);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        Array.Copy(_array, 0, array, arrayIndex, Count);
    }

    public int IndexOf(object? value)
    {
        for(int i = 0; i < Count; i++)
        {
            var o = _array[i];
            if (o is null)
            {
                if (value is null) return i;
            }else if (o.Equals(value)) return i;
        }

        return -1;
    }
    
    public int IndexOf(T item)
    {
        for(int i = 0; i < Count; i++)
        {
            var o = _array[i];
            if (o is not null && o.Equals(item)) return i;
        }

        return -1;
    }

    public void Remove(object? value)
    {
        for (int i = 0; i < Count; i++)
        {
            var o = _array[i];
            if (o is null)
            {
                if (value is null)
                {
                    RemoveAt(i);
                    return;
                }
            }
            else if (o.Equals(value))
            {
                RemoveAt(i);
                return;
            }
        }
    }
    
    public bool Remove(T item)
    {
        for (int i = 0; i < Count; i++)
        {
            var o = _array[i];
            if (o is not null && o.Equals(item))
            {
                RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public void Insert(int index, T item)
    {
        if (index < 0 || index > Count) return;
        
        GrowIfNecessary();
        
        if (index == Count)
        {
            Add(item);
            return;
        }

        Array.Copy(_array, index, _array, index + 1, Count - index);
        _array[index] = item;
        Count++;
        ElementAdded?.Invoke(item);
    }
    
    public void Insert(int index, object? value)
    {
        if (value is not T item) return;

        Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Count) return;
        
        Array.Copy(_array, index + 1, _array, index, Count - index - 1);
        Count--;
    }

    T IList<T>.this[int index]
    {
        get => _array[index];
        set => _array[index] = value;
    }

    public object? this[int index]
    {
        get => _array[index];
        set
        {
            if(value is T item) _array[index] = item;
        } 
    }
    
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return _array[i];
        }
    }

    public IEnumerator GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return _array[i];
        }
    }
    
    //Private-----------------------------------------------------------------------------------------------------------

    private void GrowIfNecessary()
    {
        if (_array.Length <= Count)
        {
            if (_array.Length == 0)
            {
                _array = new T[4];
            }
            else
            {
                var buffer = new T[_array.Length * 2];
                Array.Copy(_array, 0, buffer, 0, _array.Length);
                _array = buffer;
            }
        }
    }
}

public delegate void OnClear();
public delegate void OnElementAdded<in T>(T element);