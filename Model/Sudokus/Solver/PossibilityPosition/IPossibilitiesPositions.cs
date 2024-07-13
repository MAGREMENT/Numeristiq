using System.Collections.Generic;
using System.Text;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.PossibilityPosition;

public interface IPossibilitiesPositions
{
    ReadOnlyBitSet16 Possibilities { get; }
    GridPositions Positions { get; }
    GridPositions PositionsFor(int p);
    int PossibilityCount { get; }
    int PositionsCount { get; }
    
    IEnumerable<Cell> EnumerateCells();
    IEnumerable<Cell> EnumerateCells(int with);
    IEnumerable<CellPossibility> EnumeratePossibilities();
    
    ReadOnlyBitSet16 PossibilitiesInCell(Cell cell);
    CellPossibilities[] ToCellPossibilitiesArray();
    
    bool IsPossibilityRestricted(IPossibilitiesPositions other, int possibility);
    
    public ReadOnlyBitSet16 RestrictedCommons(IPossibilitiesPositions other)
    {
        ReadOnlyBitSet16 result = new();

        foreach (var possibility in Possibilities.EnumeratePossibilities())
        {
            if (!other.Possibilities.Contains(possibility)) continue;

            if (IsPossibilityRestricted(other, possibility)) result += possibility;
        }

        return result;
    }

    public bool Contains(CellPossibility cp)
    {
        return Possibilities.Contains(cp.Possibility) && Positions.Contains(cp.Row, cp.Column);
    }
}