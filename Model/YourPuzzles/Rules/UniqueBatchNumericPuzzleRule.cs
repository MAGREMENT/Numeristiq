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
    
    private readonly Cell[] _cells;

    public UniqueBatchNumericPuzzleRule(Cell[] cells)
    {
        _cells = cells;
    }

    public IEnumerable<Cell> EnumerateCells()
    {
        return _cells;
    }

    public IEnumerable<ISetting> EnumerateSettings()
    {
        return Enumerable.Empty<ISetting>();
    }

    public bool IsRespected(IReadOnlyNumericYourPuzzle board)
    {
        var bitSet = new ReadOnlyBitSet16();
        foreach (var cell in _cells)
        {
            var n = board[cell];
            if (n <= 0 || n > _cells.Length || bitSet.Contains(n)) return false;

            bitSet += n;
        }

        return true;
    }
    
    public bool IsStillApplicable(int rowCount, int colCount)
    {
        return ILocalNumericPuzzleRule.DefaultIsStillApplicable(this, rowCount, colCount);
    }

    public bool Overlaps(IReadOnlyList<Cell> cells)
    {
        foreach (var cell in cells)
        {
            if (_cells.Contains(cell)) return true;
        }

        return false;
    }
}

public class UniqueBatchNumericPuzzleRuleCrafter : ILocalNumericPuzzleRuleCrafter
{
    public string Name => UniqueBatchNumericPuzzleRule.OfficialName;

    public bool CanCraft(IReadOnlyNumericYourPuzzle puzzle, IReadOnlyList<Cell> cells)
    {
        if (cells.Count is <= 0 or > 9 || !puzzle.AreAllEnabled(cells)) return false;

        foreach (var local in puzzle.LocalRules)
        {
            if (local is UniqueBatchNumericPuzzleRule u && u.Overlaps(cells)) return false;
        }

        return true;
    }

    public ILocalNumericPuzzleRule Craft(IReadOnlyList<Cell> cells) => new UniqueBatchNumericPuzzleRule(cells.ToArray());
}