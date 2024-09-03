namespace Model.Utility.Collections.Lexicography;

public class LexicographicTree<T>
{
    private readonly LexicographicNode<T> _start;
    private readonly ConstructNode<T> _construct;

    public LexicographicTree(ConstructNode<T> construct)
    {
        _construct = construct;
        _start = _construct();
    }

    public void Add(string s, T value)
    {
        var node = _start;
        foreach (var c in s)
        {
            node = node.Add(c, _construct);
        }

        node.Value = value;
    }

    public bool TryGet(string s, out T? value)
    {
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

public delegate LexicographicNode<T> ConstructNode<T>();

public abstract class LexicographicNode<T>
{
    public T? Value { get; set; }
    
    public abstract LexicographicNode<T> Add(char c, ConstructNode<T> construct);
    public abstract LexicographicNode<T>? Get(char c);
}