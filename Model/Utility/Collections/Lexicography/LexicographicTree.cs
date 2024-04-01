using System;
using Model.Utility.Collections.Lexicography.Nodes;

namespace Model.Utility.Collections.Lexicography;

public class LexicographicTree<T>
{
    private readonly LexicographicNode<T> _start = ChosenNode.New<T>();

    public void TryAdd(string s, T value)
    {
        var node = _start;
        foreach (var c in s)
        {
            node = node.Add(c);
        }

        node.Value ??= value;
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

public static class ChosenNode
{
    public static LexicographicNode<T> New<T>() => new FluctuatingArrayNode<T>();
}

public abstract class LexicographicNode<T>
{
    public T? Value { get; set; }
    
    public abstract LexicographicNode<T> Add(char c);
    
    public abstract LexicographicNode<T>? Get(char c);
}

public static class LexicographicAlphabet
{
    public static int Count => 50; //a-z + A-Z + ' ' + '-'

    public static int ToInt(char c)
    {
        return c switch
        {
            ' ' => 48,
            '-' => 49,
            >= 'a' and <= 'z' => c - 'a',
            >= 'A' and <= 'Z' => c - 'A' + 24,
            _ => throw new ArgumentOutOfRangeException(nameof(c))
        };
    }

    public static char ToChar(int n)
    {
        return n switch
        {
            48 => ' ',
            49 => '-',
            >= 0 and <= 23 => (char)('a' + n),
            <= 47 => (char)('A' + n),
            _ => throw new ArgumentOutOfRangeException(nameof(n))
        };
    }
}