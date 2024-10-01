using System.Collections.Generic;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;
using Model.Utility.Collections;

namespace Model.Sudokus.Solver.PossibilitySets;

public interface IPossibilitySet : ISudokuElement
{
    int ISudokuElement.DifficultyRank => 3;
    
    GridPositions Positions { get; }
    GridPositions PositionsFor(int p);
    int PossibilityCount { get; }
    int PositionsCount { get; }
    
    IEnumerable<Cell> EnumerateCells(int with);
    ReadOnlyBitSet16 PossibilitiesInCell(Cell cell);
    
    bool IsPossibilityRestricted(IPossibilitySet other, int possibility);
    public ReadOnlyBitSet16 RestrictedCommons(IPossibilitySet other)
    {
        ReadOnlyBitSet16 result = new();

        foreach (var possibility in EnumeratePossibilities())
        {
            if (!other.Contains(possibility)) continue;

            if (IsPossibilityRestricted(other, possibility)) result += possibility;
        }

        return result;
    }

    public static bool InternalEquals(IPossibilitySet s, object? obj)
    {
        if (obj is not IPossibilitySet set || set.PositionsCount != s.PositionsCount) return false;

        foreach (var cp in s.EnumerateCellPossibilities())
        {
            if (!set.Contains(cp)) return false;
        }

        return true;
    }

    public static int InternalHash(IPossibilitySet s)
    {
        int hash = 0;
        foreach (var cp in s.EnumerateCellPossibilities())
        {
            hash ^= cp.GetHashCode();
        }

        return hash;
    }
    
    public static string InternalToString(IPossibilitySet s)
    {
        return s.EveryPossibilities().ToValuesString() + "{" + s.EnumerateCells().ToStringSequence(" ") + "}";
    }
}