using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace Model.Utility.Collections;

public class ListSpan<T> : IEnumerable<T>
{
    private readonly IReadOnlyList<T> _list;
    private readonly int[] _indexes;

    public static IEnumerable<T> Merge(IEnumerable<ListSpan<T>> spans)
    {
        var ind = new HashSet<int>();
        IReadOnlyList<T>? list = null;
        foreach (var span in spans)
        {
            list ??= span._list;
            ind.UnionWith(span._indexes);
        }

        return list is null ? Enumerable.Empty<T>() : new ListSpan<T>(list, ind.ToArray());
    }

    public ListSpan(IReadOnlyList<T> list, params int[] indexes)
    {
        _list = list;
        _indexes = indexes;
    }

    public IEnumerable<(T, int)> EnumerateWithIndex()
    {
        for (int i = 0; i < _indexes.Length; i++)
        {
            yield return (_list[_indexes[i]], _indexes[i]);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var i in _indexes)
        {
            if (i >= 0 || i < _list.Count) yield return _list[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public T this[int index] => _list[_indexes[index]];

    public int Count => _indexes.Length;
}

public class NamedListSpan<T> : ListSpan<T>, INamed
{
    public string Name { get; }
    
    public NamedListSpan(string name, IReadOnlyList<T> list, params int[] indexes) : base(list, indexes)
    {
        Name = name;
    }
}

public interface INamed
{
    string Name { get; }
}