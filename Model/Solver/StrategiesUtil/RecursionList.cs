using System;
using System.Collections;
using System.Collections.Generic;

namespace Model.Solver.StrategiesUtil;

public class RecursionList<T> : IEnumerable<T> //TODO use everywhere
{
    private const int DefaultSize = 8;

    public int Size { get; private set; }
    public int Cursor
    {
        get => _cursor;
        set
        {
            if (value < 0 || value >= Size) return;
            _cursor = value;
        }
    }

    private T[] _array;
    private readonly int _growth;
    private int _cursor;

    public RecursionList()
    {
        _array = new T[DefaultSize];
        _growth = DefaultSize >> 1;
    }

    public RecursionList(int capacity)
    {
        if (capacity < 0) capacity = 0;
        _array = new T[capacity];
        _growth = capacity >> 1;
    }

    public void Add(T element)
    {
        if (Size == _array.Length)
        {
            var buffer = new T[_array.Length + _growth];
            Array.Copy(_array, 0, buffer, 0, Size);
            _array = buffer;
        }

        _array[Size] = element;
        Size++;
    }

    public void SetAtCursor(T element)
    {
        _array[Cursor] = element;
    }

    public void SetCursorAndSetOrAdd(int cursor, T element)
    {
        if (cursor > Size || cursor < 0) return;
        if (cursor == Size)
        {
            Add(element);
            Cursor = Size - 1;
            return;
        }

        Cursor = cursor;
        SetAtCursor(element);
    }

    public T this[int i] => _array[i];

    public T[] CopyUntilCursor()
    {
        var buffer = new T[Cursor + 1];
        Array.Copy(_array, 0, buffer, 0, Cursor + 1);
        return buffer;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private struct Enumerator : IEnumerator<T>
    {
        private readonly int _max;
        private readonly T[] _array;
        private int _cursor = -1;

        public Enumerator(RecursionList<T> list)
        {
            _max = list.Cursor;
            _array = list._array;
        }

        public bool MoveNext()
        {
            _cursor++;
            return _cursor <= _max;
        }

        public void Reset()
        {
            _cursor = -1;
        }

        public T Current => _array[_cursor];

        object IEnumerator.Current => Current!;

        public void Dispose()
        {
            
        }
    }
}