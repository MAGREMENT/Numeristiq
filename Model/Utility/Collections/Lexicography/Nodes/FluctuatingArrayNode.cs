using System;

namespace Model.Utility.Collections.Lexicography.Nodes;

public class FluctuatingArrayNode<T> : LexicographicNode<T>
{
    private int _count = 0;
    private int[] _indexes = Array.Empty<int>();
    private LexicographicNode<T>[] _nodes = Array.Empty<LexicographicNode<T>>();
    
    public override LexicographicNode<T> Add(char c)
    {
        var i = IndexOf(c);
        if (i != -1) return _nodes[i];
        
        GrowIfNecessary();
        
        _indexes[_count] = LexicographicAlphabet.ToInt(c);
        var node = ChosenNode.New<T>();
        _nodes[_count] = node;
        _count++;
        
        return node;
    }

    public override LexicographicNode<T>? Get(char c)
    {
        var i = IndexOf(c);
        return i == -1 ? null : _nodes[i];
    }

    private int IndexOf(char c)
    {
        var n = LexicographicAlphabet.ToInt(c);
        for (int i = 0; i < _count; i++)
        {
            if (_indexes[i] == n) return i;
        }

        return -1;
    }

    private void GrowIfNecessary()
    {
        if (_count < _indexes.Length) return;

        var newSize = _indexes.Length == 0 ? 4 : _indexes.Length * 2;
        var bi = new int[newSize];
        var bn = new LexicographicNode<T>[newSize];
        
        Array.Copy(_indexes, 0, bi, 0, _indexes.Length);
        Array.Copy(_nodes, 0, bn, 0, _nodes.Length);

        _indexes = bi;
        _nodes = bn;
    }
}