using System.Collections.Generic;
using System.Linq;
using Model.Core.Settings;
using Model.Utility;
using Model.Utility.BitSets;
using Model.Utility.Collections;

namespace Model.YourPuzzles.Rules;

public class NonRepeatingBatchNumericPuzzleRule : ILocalNumericPuzzleRule
{
    public const string OfficialName = "Non-Repeating Batch";
    public const string OfficialAbbreviation = "nb";

    public string Name => OfficialName;
    
    public Cell[] Cells { get; }

    public NonRepeatingBatchNumericPuzzleRule(Cell[] cells)
    {
        Cells = cells;
    }

    public IEnumerable<Cell> EnumerateCells()
    {
        return Cells;
    }

    public string Abbreviation => OfficialAbbreviation;

    public IEnumerable<ISetting> EnumerateSettings()
    {
        return Enumerable.Empty<ISetting>();
    }

    public bool IsRespected(IReadOnlyNumericYourPuzzle board)
    {
        var bitSet = new ReadOnlyBitSet16();
        foreach (var cell in Cells)
        {
            var n = board[cell];
            if (n <= 0 || n > Cells.Length || bitSet.Contains(n)) return false;

            bitSet += n;
        }

        return true;
    }

    public string DataToString()
    {
        return Cells.ToStringSequence("");
    }

    public bool IsStillApplicable(int rowCount, int colCount)
    {
        return ILocalNumericPuzzleRule.DefaultIsStillApplicable(this, rowCount, colCount);
    }

    public bool Overlaps(IEnumerable<Cell> cells)
    {
        foreach (var cell in cells)
        {
            if (Cells.Contains(cell)) return true;
        }

        return false;
    }
}

public class NonRepeatingBatchNumericPuzzleRuleCrafter : ILocalNumericPuzzleRuleCrafter
{
    public string Name => NonRepeatingBatchNumericPuzzleRule.OfficialName;

    public bool CanCraft(IReadOnlyNumericYourPuzzle puzzle, IReadOnlyList<Cell> cells)
    {
        if (cells.Count is <= 1 or > 9 || !CellUtility.AreAllAdjacent(cells) 
                                       || !puzzle.AreAllEnabled(cells)) return false;

        foreach (var local in puzzle.LocalRules.ForAll<NonRepeatingBatchNumericPuzzleRule>())
        {
            if (local.Overlaps(cells)) return false;
        }

        return true;
    }

    public ILocalNumericPuzzleRule Craft(IReadOnlyList<Cell> cells) => new NonRepeatingBatchNumericPuzzleRule(cells.ToArray());
    public string Abbreviation => NonRepeatingBatchNumericPuzzleRule.OfficialAbbreviation;
    public INumericPuzzleRule? Craft(string s)
    {
        var c = s.TryReadCells();
        return c is null ? null : new NonRepeatingBatchNumericPuzzleRule(c.ToArray());
    }
}