﻿using System.Text;

namespace Model.Utility;

public interface IStringConverter
{
    public string Convert(string s);
}

public class SpacedToCamelCaseConverter : IStringConverter
{
    private readonly StringBuilder _builder = new();

    public static SpacedToCamelCaseConverter Instance { get; } = new();

    public string Convert(string? s)
    {
        if (s is null) return string.Empty;

        _builder.Clear();
        var shouldBeUpper = true;
        foreach (var c in s)
        {
            if (c == ' ')
            {
                shouldBeUpper = true;
                continue;
            }

            _builder.Append(shouldBeUpper ? char.ToUpper(c) : c);
            shouldBeUpper = false;
        }

        return _builder.ToString();
    }
}

public class CamelCaseToSpacedConverter : IStringConverter
{
    private readonly StringBuilder _builder = new();

    public static CamelCaseToSpacedConverter Instance { get; } = new();
    
    public string Convert(string? s)
    {
        if (s is null) return string.Empty;
        if (s.Length is 0 or 1) return s;
        
        _builder.Clear();
        _builder.Append(s[0]);

        for (int i = 1; i < s.Length; i++)
        {
            var c = s[i];
            if (char.IsUpper(c))
            {
                _builder.Append(' ');
                _builder.Append(char.ToLower(c));
            }
            else _builder.Append(c);
        }

        return _builder.ToString();
    }
}