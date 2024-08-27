using System.Collections.Generic;
using System.Linq;
using Model.Core.Settings;
using Model.Utility;

namespace Model.YourPuzzles.Rules;

public class GreaterThanNumericPuzzleRule : ILocalNumericPuzzleRule
{
    public const string OfficialName = "Greater Than";

    public string Name => OfficialName;
    
    public Cell Greater { get; }
    public Cell Smaller { get; }

    public GreaterThanNumericPuzzleRule(Cell greater, Cell smaller)
    {
        Greater = greater;
        Smaller = smaller;
    }

    public IEnumerable<Cell> EnumerateCells()
    {
        yield return Greater;
        yield return Smaller;
    }

    public IEnumerable<ISetting> EnumerateSettings()
    {
        return Enumerable.Empty<ISetting>();
    }

    public bool IsRespected(IReadOnlyNumericYourPuzzle board)
    {
        var gc = board[Greater];
        if (gc <= 0) return false;

        var sc = board[Smaller];
        if (sc <= 0) return false;

        return gc > sc;
    }

    public bool IsStillApplicable(int rowCount, int colCount)
    {
        return ILocalNumericPuzzleRule.DefaultIsStillApplicable(this, rowCount, colCount);
    }
}

public class GreaterThanNumericPuzzleRuleCrafter : ILocalNumericPuzzleRuleCrafter
{
    public string Name => GreaterThanNumericPuzzleRule.OfficialName;

    public bool CanCraft(IReadOnlyNumericYourPuzzle puzzle, IReadOnlyList<Cell> cells)
    {
        if (cells.Count != 2 || !puzzle.AreAllEnabled(cells)) return false;

        foreach (var local in puzzle.LocalRules)
        {
            if(local is GreaterThanNumericPuzzleRule g && cells.Contains(g.Smaller)
               && cells.Contains(g.Greater)) return false;
        }

        return true;
    }

    public ILocalNumericPuzzleRule Craft(IReadOnlyList<Cell> cells) =>
        new GreaterThanNumericPuzzleRule(cells[0], cells[1]);
}