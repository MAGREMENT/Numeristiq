using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Model.YourPuzzles.Syntax.Parsables;

public abstract class FormatParsable : ISyntaxParsable
{
    private readonly Regex _regex;
    
    public string? IsSimpleString => null;
    
    protected FormatParsable(string s)
    {
        _regex = new Regex(s);
    }
    
    public ISyntaxElement? Parse(string s)
    {
        var result = _regex.Match(s);
        if (!result.Success) return null;

        var list = new List<Group>();
        foreach (var g in result.Groups)
        {
            if (g is Group grp) list.Add(grp);
        }
        
        return Parse(list);
    }

    protected abstract ISyntaxElement? Parse(IReadOnlyList<Group> collection);
}