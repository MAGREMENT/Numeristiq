using System.Collections.Generic;
using System.Text.RegularExpressions;
using Model.Core.Syntax.Parsables;
using Model.Sudokus.Player;
using Model.Utility;

namespace Model.Core.Syntax.Values;

public class CellValue : SyntaxValue
{
    private readonly Cell _cell;

    public CellValue(Cell cell)
    {
        _cell = cell;
    }

    protected override HighlightColor GetHighlight() => HighlightColor.Second;
    protected override string GetString() => _cell.ToString();
}

public class CellParsable : FormatParsable<ISyntaxElement>
{
    public CellParsable() : base(@"^r(\d+)c(\d+)$")
    {
    }

    protected override ISyntaxElement? Parse(IReadOnlyList<Group> collection)
    {
        if (collection.Count != 2
            || !int.TryParse(collection[0].Value, out var r)
            || !int.TryParse(collection[1].Value, out var c)) return null;

        return new CellValue(new Cell(r, c));
    }
}
