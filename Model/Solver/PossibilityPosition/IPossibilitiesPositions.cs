using System.Collections.Generic;
using Global;
using Model.Solver.Position;
using Model.Solver.Possibility;
using Model.Solver.StrategiesUtility;

namespace Model.Solver.PossibilityPosition;

public interface IPossibilitiesPositions
{
    IEnumerable<Cell> EachCell();
    IEnumerable<Cell> EachCell(int with);
    
    IReadOnlyPossibilities PossibilitiesInCell(Cell cell);
    
    IReadOnlyPossibilities Possibilities { get; }
    GridPositions Positions { get; }

    CellPossibilities[] ToCellPossibilitiesArray();
    
    int PossibilityCount { get; }
    int PositionsCount { get; }
    
    public Possibilities RestrictedCommons(IPossibilitiesPositions other)
    {
        Possibilities result = Possibility.Possibilities.NewEmpty();

        foreach (var possibility in Possibilities)
        {
            if (!other.Possibilities.Peek(possibility)) continue;

            if (IsPossibilityRestricted(other, possibility)) result.Add(possibility);
        }

        return result;
    }

    private bool IsPossibilityRestricted(IPossibilitiesPositions other, int possibility)
    {
        foreach (var cell1 in EachCell(possibility))
        {
            foreach (var cell2 in other.EachCell(possibility))
            {
                if (!Cells.ShareAUnit(cell1, cell2)) return false;
            }
        }

        return true;
    }
}