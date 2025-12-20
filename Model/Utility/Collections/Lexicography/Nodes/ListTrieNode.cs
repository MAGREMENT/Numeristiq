using System.Collections.Generic;

namespace Model.Utility.Collections.Lexicography.Nodes;

public class ListTrieNode<T> : TrieNode<T>
{
    private readonly List<(char, TrieNode<T>)> _list = new();
    
    public override TrieNode<T> Add(char c, ITrieNodeConstructor<T> constructor)
    {
        var n = Get(c);
        if (n is not null) return n;

        n = constructor.Construct();
        _list.Add((c, n));
        return n;
    }

    public override TrieNode<T>? Get(char c)
    {
        foreach (var (ch, n) in _list)
        {
            if (ch == c) return n;
        }

        return null;
    }
}