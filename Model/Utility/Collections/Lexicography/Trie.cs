using Model.Utility.Collections.Lexicography.Nodes;

namespace Model.Utility.Collections.Lexicography;

public abstract class Trie<T> : ITrieNodeConstructor<T>
{
    private TrieNode<T>? _start;

    public abstract TrieNode<T> Construct();
    
    public bool Add(string s, T value)
    {
        _start ??= Construct();
        
        var node = _start;
        foreach (var c in s)
        {
            node = node.Add(c, this);
        }

        if (node.HasValue) return false;
        
        node.SetValue(value);
        return true;
    }

    public bool TryGet(string s, out T? value)
    {
        if (_start is null)
        {
            value = default;
            return false;
        }
        
        var node = _start;
        foreach (var c in s)
        {
            node = node.Get(c);
            if (node is null)
            {
                value = default;
                return false;
            }
        }

        value = node.Value;
        return value is not null;
    }
}

public class ListTrie<T> : Trie<T>
{
    public override TrieNode<T> Construct() => new ListTrieNode<T>();
}

public interface ITrieNodeConstructor<T>
{
    TrieNode<T> Construct();
}

public abstract class TrieNode<T>
{
    public T? Value { get; private set; }
    public bool HasValue { get; private set; }

    public void SetValue(T val)
    {
        Value = val;
        HasValue = true;
    }
    
    public abstract TrieNode<T> Add(char c, ITrieNodeConstructor<T> constructor);
    public abstract TrieNode<T>? Get(char c);
}