using System.Collections.Generic;
using System.Text.RegularExpressions;
using Model.Sudokus.Player;
using Model.Utility;
using Model.YourPuzzles.Syntax.Parsables;

namespace Model.YourPuzzles.Syntax.Values;

public class CellValue : ISyntaxValue
{
    private readonly Cell _cell;

    public CellValue(Cell cell)
    {
        _cell = cell;
    }

    public SyntaxString ToSyntaxString()
    {
        return new SyntaxString(_cell.ToString(), HighlightColor.Second);
    }
    
    public override string ToString()
    {
        return _cell.ToString();
    }
}

public class CellParsable : FormatParsable
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
