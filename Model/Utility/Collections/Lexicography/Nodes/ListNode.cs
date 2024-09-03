using System.Collections.Generic;

namespace Model.Utility.Collections.Lexicography.Nodes;

public class ListNode<T> : LexicographicNode<T>
{
    private readonly List<(char, LexicographicNode<T>)> _list = new();
    
    public override LexicographicNode<T> Add(char c, ConstructNode<T> construct)
    {
        var n = Get(c);
        if (n is not null) return n;

        n = construct();
        _list.Add((c, n));
        return n;
    }

    public override LexicographicNode<T>? Get(char c)
    {
        foreach (var (ch, n) in _list)
        {
            if (ch == c) return n;
        }

        return null;
    }
}