using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Model.Util;

public static class IniFileReader
{
    public static Dictionary<string, IniFileSection> Read(string path)
    {
        var builder = new IniFileResultBuilder();

        using TextReader reader = new StreamReader(path, Encoding.UTF8);

        while (reader.ReadLine() is { } line)
        {
            if(line.Length == 0) continue;
            var firstChar = line[0];

            switch (firstChar)
            {
                case '#' :
                    break;
                case '[' :
                    if (line[^1] != ']') continue;
                    
                    builder.AddSection(line.Substring(1, line.Length - 2));
                    break;
                default:
                    var split = line.Split('=');
                    if (split.Length != 2) continue;

                    builder.AddEntry(split[0], split[1]);
                    break;
            }
        }

        return builder.GetResult();
    }
}

public class IniFileResultBuilder
{
    private readonly Dictionary<string, IniFileSection> _sections = new();
    private IniFileSection? _last;

    public void AddSection(string name)
    {
        var buffer = new IniFileSection();
        _sections.Add(name, buffer);
        _last = buffer;
    }

    public void AddEntry(string key, string value)
    {
        if (_last is null) return;
        _last.AddEntry(key, value);
    }

    public Dictionary<string, IniFileSection> GetResult()
    {
        return new Dictionary<string, IniFileSection>(_sections);
    }
}

public class IniFileSection : IEnumerable<Entry<string, string>>
{
    private readonly List<Entry<string, string>> _entries = new();

    public void AddEntry(string key, string value)
    {
        _entries.Add(new Entry<string, string>(key, value));
    }
    
    public IEnumerator<Entry<string, string>> GetEnumerator()
    {
        return _entries.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class Entry<TK, TV>
{
    public Entry(TK key, TV value)
    {
        Key = key;
        Value = value;
    }

    public TK Key { get; }
    public TV Value { get; }
}