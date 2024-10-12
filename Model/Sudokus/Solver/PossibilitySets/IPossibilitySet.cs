using System.Collections.Generic;
using System.Linq;
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
        return obj is IPossibilitySet set && set.EveryPossibilities() == s.EveryPossibilities()
                                          && set.PositionsCount == s.PositionsCount
                                          && s.EveryCell().SequenceEqual(set.EveryCell());
    }

    public static int InternalHash(IPossibilitySet s)
    {
        int hash = s.EveryPossibilities().GetHashCode();
        foreach (var cell in s.EveryCell())
        {
            hash ^= cell.GetHashCode();
        }

        return hash;
    }
    
    public static string InternalToString(IPossibilitySet s)
    {
        return s.EveryPossibilities().ToValuesString() + "{" + s.EnumerateCells().ToStringSequence(" ") + "}";
    }
}