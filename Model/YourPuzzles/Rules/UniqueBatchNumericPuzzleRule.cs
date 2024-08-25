using System.Collections.Generic;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.YourPuzzles.Rules;

public class UniqueBatchNumericPuzzleRule : INumericPuzzleRule
{
    private readonly Cell[] _cells;

    public UniqueBatchNumericPuzzleRule(Cell[] cells)
    {
        _cells = cells;
    }

    public IEnumerable<Cell> EnumerateCells()
    {
        return _cells;
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
        return INumericPuzzleRule.DefaultIsStillApplicable(this, rowCount, colCount);
    }
}