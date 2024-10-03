using System;

namespace Model.Utility;

public interface IAlphabet
{
    public int Count { get; }
    public int ToInt(char c);
    public char ToChar(int n);
    public bool Contains(char c);
}

public static class AlphabetExtensions
{
    public static bool Contains(this IAlphabet alphabet, string s)
    {
        foreach (var c in s)
        {
            if (!alphabet.Contains(c)) return false;
        }

        return true;
    }
}

public class RFC4648Base32Alphabet : IAlphabet
{
    public static RFC4648Base32Alphabet Instance { get; } = new();
    
    private RFC4648Base32Alphabet(){}
    
    public int Count => 32;
    
    public int ToInt(char c)
    {
        //65-90 == uppercase letters
        if (c < 91 && c > 64)
        {
            return c - 65;
        }
        //50-55 == numbers 2-7
        if (c < 56 && c > 49)
        {
            return c - 24;
        }

        return 0;
    }

    public char ToChar(int n)
    {
        return n switch
        {
            < 26 => (char)(n + 'A'),
            < 32 => (char)(n + 24),
            _ => 'A'
        };
    }

    public bool Contains(char c)
    {
        return (c < 91 && c > 64) || (c < 56 && c > 49);
    }
}

public class HexadecimalAlphabet : IAlphabet
{
    public static HexadecimalAlphabet Instance { get; } = new();
    
    private HexadecimalAlphabet(){}
    public int Count => 16;

    public int ToInt(char c)
    {
        return c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'A' and <= 'F' => c - 'A' + 10,
            _ => throw new ArgumentOutOfRangeException(nameof(c))
        };
    }

    public char ToChar(int n)
    {
        if (n > 9) return (char)('A' + n - 10);
        return (char)('0' + n);
    }

    public bool Contains(char c)
    {
        return (c is >= '0' and <= '9' or >= 'A' and <= 'F');
    }
}

public class DefaultBase16Alphabet : IAlphabet
{
    public static DefaultBase16Alphabet Instance { get; } = new();
    
    private DefaultBase16Alphabet(){}
    public int Count => 16;
    public int ToInt(char c)
    {
        return c - 'a';
    }

    public char ToChar(int n)
    {
        return (char)('a' + n);
    }

    public bool Contains(char c)
    {
        return c is >= 'a' and <= (char)('a' + 16);
    }
}

public class DefaultBase32Alphabet : IAlphabet
{
    public static DefaultBase32Alphabet Instance { get; } = new();
    
    private DefaultBase32Alphabet(){}
    
    public int Count => 32;
    
    public int ToInt(char c)
    {
        return c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'a' and <= 'w' => c - 'a' + 10,
            _ => '0'
        };
    }

    public char ToChar(int n)
    {
        if (n < 10) return (char)(n + '0');
        return (char)(n - 10 + 'a');
    }

    public bool Contains(char c)
    {
        return c is >= '0' and <= '9' or >= 'a' and <= 'w';
    }
}

public class LexicographicAlphabet : IAlphabet
{
    public static LexicographicAlphabet Instance { get; } = new();
    
    private LexicographicAlphabet(){}
    
    public int Count => 50; //a-z + A-Z + ' ' + '-'

    public int ToInt(char c)
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

    public char ToChar(int n)
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

    public bool Contains(char c)
    {
        return c == 48 || c == 49 || c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
    }
}