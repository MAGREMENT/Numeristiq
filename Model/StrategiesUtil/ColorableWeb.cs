using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Model.StrategiesUtil;

public class ColorableWeb<T> : IEnumerable<T> where T : class, IColorable //Refactor this bullshit
{
    private readonly Dictionary<T, HashSet<T>> _vertices = new();
    private bool _isColored = false;

    public int Count => _vertices.Count;

    public bool Contains(T coord)
    {
        return _vertices.ContainsKey(coord);
    }

    public bool AddLink(T one, T two)
    {
        if (_isColored) return false;
        if (_vertices.ContainsKey(one) && _vertices.ContainsKey(two)) return false;

        if (!_vertices.TryAdd(one, new HashSet<T> { two }))
        {
            _vertices[one].Add(two);
        }
        
        if (!_vertices.TryAdd(two, new HashSet<T> { one }))
        {
            _vertices[two].Add(one);
        }

        return true;
    }

    public void StartColoring()
    {
        if (_isColored) return;
        
        T start = _vertices.Keys.First();
        start.Coloring = Coloring.On;
        Color(start);
        
        _isColored = true;
    }

    private void Color(T current)
    {
        Coloring opposite = current.Coloring == Coloring.On ? Coloring.Off : Coloring.On;
        foreach (var linkedTo in _vertices[current])
        {
            if (linkedTo.Coloring == Coloring.None)
            {
                linkedTo.Coloring = opposite;
                Color(linkedTo);
            }
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _vertices.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public delegate bool HandleCombination(T one, T two);

    public void ForEachCombinationOfTwo(HandleCombination handler)
    {
        Queue<T> queue = new Queue<T>(_vertices.Keys);
        while (queue.Count > 0)
        {
            T one = queue.Dequeue();
            Queue<T> copy = new Queue<T>(queue);
            while (copy.Count > 0)
            {
                if(handler(one, copy.Dequeue())) return;
            }
        }
    }

    public HashSet<T> GetLinkedVertices(T vertex)
    {
        return _vertices[vertex];
    }
}