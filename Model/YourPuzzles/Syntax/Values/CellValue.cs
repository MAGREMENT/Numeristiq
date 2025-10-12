using Model.Sudokus.Player;
using Model.Utility;

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
}