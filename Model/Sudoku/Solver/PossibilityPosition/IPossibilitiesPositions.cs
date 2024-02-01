using System.Collections.Generic;
using Model.Sudoku.Solver.Position;
using Model.Sudoku.Solver.Possibility;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;

namespace Model.Sudoku.Solver.PossibilityPosition;

public interface IPossibilitiesPositions
{
    IReadOnlyPossibilities Possibilities { get; }
    GridPositions Positions { get; }
    int PossibilityCount { get; }
    int PositionsCount { get; }
    
    IEnumerable<Cell> EachCell();
    IEnumerable<Cell> EachCell(int with);
    IReadOnlyPossibilities PossibilitiesInCell(Cell cell);

    CellPossibilities[] ToCellPossibilitiesArray();
    bool IsPossibilityRestricted(IPossibilitiesPositions other, int possibility);
    
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

    public bool Contains(CellPossibility cp)
    {
        return Possibilities.Peek(cp.Possibility) && Positions.Peek(cp.Row, cp.Column);
    }
}