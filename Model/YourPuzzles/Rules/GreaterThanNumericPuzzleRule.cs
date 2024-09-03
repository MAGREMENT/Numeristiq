using System.Collections.Generic;
using System.Linq;
using Model.Core.Settings;
using Model.Utility;

namespace Model.YourPuzzles.Rules;

public class GreaterThanNumericPuzzleRule : ILocalNumericPuzzleRule
{
    public const string OfficialName = "Greater Than";
    public const string OfficialAbbreviation = "gt";

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

    public string Abbreviation => OfficialAbbreviation;

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

    public string DataToString()
    {
        return $"{Greater}>{Smaller}";
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
        if (cells.Count != 2 || !puzzle.AreAllEnabled(cells) 
                             || !cells[0].IsAdjacentTo(cells[1])) return false;

        foreach (var local in puzzle.LocalRules)
        {
            if(local is GreaterThanNumericPuzzleRule g && cells.Contains(g.Smaller)
               && cells.Contains(g.Greater)) return false;
        }

        return true;
    }

    public ILocalNumericPuzzleRule Craft(IReadOnlyList<Cell> cells) =>
        new GreaterThanNumericPuzzleRule(cells[0], cells[1]);

    public string Abbreviation => GreaterThanNumericPuzzleRule.OfficialAbbreviation;
    public INumericPuzzleRule? Craft(string s)
    {
        var index = s.IndexOf('>');
        if (index == -1 || !s.TryReadCell(0, index, out var greater)
                        || !s.TryReadCell(index + 1, s.Length, out var smaller)) return null;

        return new GreaterThanNumericPuzzleRule(greater, smaller);
    }
}