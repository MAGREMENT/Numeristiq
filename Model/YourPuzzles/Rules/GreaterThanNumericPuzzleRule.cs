using System.Collections.Generic;
using Model.Utility;

namespace Model.YourPuzzles.Rules;

public class GreaterThanNumericPuzzleRule : INumericPuzzleRule
{
    private readonly Cell _greater;
    private readonly Cell _smaller;

    public GreaterThanNumericPuzzleRule(Cell greater, Cell smaller)
    {
        _greater = greater;
        _smaller = smaller;
    }

    public IEnumerable<Cell> EnumerateCells()
    {
        yield return _greater;
        yield return _smaller;
    }

    public bool IsRespected(IReadOnlyNumericYourPuzzle board)
    {
        var gc = board[_greater];
        if (gc <= 0) return false;

        var sc = board[_smaller];
        if (sc <= 0) return false;

        return gc > sc;
    }

    public bool IsStillApplicable(int rowCount, int colCount)
    {
        return INumericPuzzleRule.DefaultIsStillApplicable(this, rowCount, colCount);
    }
}