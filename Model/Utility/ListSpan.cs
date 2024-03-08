using System.Collections;
using System.Collections.Generic;

namespace Model.Utility;

public class ListSpan<T> : IEnumerable<T>
{
    private readonly IReadOnlyList<T> _list;
    private readonly int[] _indexes;

    public ListSpan(IReadOnlyList<T> list, params int[] indexes)
    {
        _list = list;
        _indexes = indexes;
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