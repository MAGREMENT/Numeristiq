using System.Collections.Generic;
using System.Text.RegularExpressions;
using Model.Sudokus.Player;
using Model.YourPuzzles.Syntax.Parsables;

namespace Model.YourPuzzles.Syntax.Values;

public class IntValue : ISyntaxValue
{
    private readonly int _i;

    public IntValue(int i)
    {
        _i = i;
    }

    public SyntaxString ToSyntaxString()
    {
        return new SyntaxString(_i.ToString(), HighlightColor.Second);
    }

    public override string ToString()
    {
        return _i.ToString();
    }
}

public class IntParsable : FormatParsable
{
    public IntParsable() : base(@"\d+$")
    {
    }

    protected override ISyntaxElement? Parse(IReadOnlyList<Group> collection)
    {
        if (collection.Count != 1
            || !int.TryParse(collection[0].Value, out var r)) return null;

        return new IntValue(r);
    }
}