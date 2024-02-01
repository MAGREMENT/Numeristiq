using System.Collections.Generic;
using Model.Utility;

namespace Model.Sudoku.Solver.Position;

public interface IReadOnlyLinePositions : IEnumerable<int>
{
    public int Count { get; }
    public int First();
    public int First(int except);
    public int Next(ref int cursor);

    public bool Peek(int i);
    public LinePositions Or(IReadOnlyLinePositions pos);
    public LinePositions And(IReadOnlyLinePositions pos);
    public LinePositions Difference(IReadOnlyLinePositions pos);
    public bool AreAllInSameMiniGrid();
    public int MiniGridCount();
    public LinePositions Copy();
    public string ToString(Unit unit, int unitNumber);
    public Cell[] ToCellArray(Unit unit, int unitNumber);

    public static LinePositions DefaultOr(IReadOnlyLinePositions one, IReadOnlyLinePositions two)
    {
        var result = new LinePositions();
        for (int i = 0; i < 9; i++)
        {
            if (one.Peek(i) || two.Peek(i)) result.Add(i);
        }

        return result;
    }
    
    public static LinePositions DefaultAnd(IReadOnlyLinePositions one, IReadOnlyLinePositions two)
    {
        var result = new LinePositions();
        for (int i = 0; i < 9; i++)
        {
            if (one.Peek(i) && two.Peek(i)) result.Add(i);
        }

        return result;
    }

    public static LinePositions DefaultDifference(IReadOnlyLinePositions one, IReadOnlyLinePositions two)
    {
        var result = one.Copy();
        foreach (var n in two)
        {
            result.Remove(n);
        }

        return result;
    }
}