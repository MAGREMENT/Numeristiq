using System;

namespace Model.Utility;

public interface IAlphabet
{
    public int Count { get; }
    public int ToInt(char c);
    public char ToChar(int n);
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
}