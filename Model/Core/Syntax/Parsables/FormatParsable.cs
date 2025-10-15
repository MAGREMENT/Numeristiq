using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Model.Core.Syntax.Parsables;

public abstract class FormatParsable<T> : ISyntaxParsable<T> where T : ISyntaxElement
{
    private readonly Regex _regex;
    
    public string? IsSimpleString => null;
    
    protected FormatParsable(string s)
    {
        _regex = new Regex(s);
    }
    
    public T? Parse(string s)
    {
        var result = _regex.Match(s);
        if (!result.Success) return default;

        var list = new List<Group>();
        foreach (var g in result.Groups)
        {
            if (g is Group grp) list.Add(grp);
        }
        
        return Parse(list);
    }

    protected abstract T? Parse(IReadOnlyList<Group> collection);
}