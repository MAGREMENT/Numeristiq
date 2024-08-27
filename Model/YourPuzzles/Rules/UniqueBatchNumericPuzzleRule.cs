using System.Collections.Generic;
using System.Linq;
using Model.Core.Settings;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.YourPuzzles.Rules;

public class UniqueBatchNumericPuzzleRule : ILocalNumericPuzzleRule
{
    public const string OfficialName = "Unique Batch";

    public string Name => OfficialName;
    
    public Cell[] Cells { get; }

    public UniqueBatchNumericPuzzleRule(Cell[] cells)
    {
        Cells = cells;
    }

    public IEnumerable<Cell> EnumerateCells()
    {
        return Cells;
    }

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

public class UniqueBatchNumericPuzzleRuleCrafter : ILocalNumericPuzzleRuleCrafter
{
    public string Name => UniqueBatchNumericPuzzleRule.OfficialName;

    public bool CanCraft(IReadOnlyNumericYourPuzzle puzzle, IReadOnlyList<Cell> cells)
    {
        if (cells.Count is <= 0 or > 9 || !puzzle.AreAllEnabled(cells)) return false; //TODO check for adjacency

        foreach (var local in puzzle.LocalRules)
        {
            if (local is UniqueBatchNumericPuzzleRule u && u.Overlaps(cells)) return false;
        }

        return true;
    }

    public ILocalNumericPuzzleRule Craft(IReadOnlyList<Cell> cells) => new UniqueBatchNumericPuzzleRule(cells.ToArray());
}