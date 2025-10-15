using System.Collections.Generic;
using System.Text.RegularExpressions;
using Model.Core.Syntax.Parsables;
using Model.Sudokus.Player;

namespace Model.Core.Syntax.Values;

public class IntValue : SyntaxValue
{
    private readonly int _i;

    public IntValue(int i)
    {
        _i = i;
    }

    protected override HighlightColor GetHighlight() => HighlightColor.Second;
    protected override string GetString() => _i.ToString();
}

public class IntParsable : FormatParsable<ISyntaxElement>
{
    public IntParsable() : base(@"^\d+$")
    {
    }

    protected override ISyntaxElement? Parse(IReadOnlyList<Group> collection)
    {
        if (collection.Count != 1
            || !int.TryParse(collection[0].Value, out var r)) return null;

        return new IntValue(r);
    }
}