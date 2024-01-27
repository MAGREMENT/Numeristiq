using Model.SudokuSolving.Solver.Position;
using Model.SudokuSolving.Solver.StrategiesUtility;

namespace Model.SudokuSolving.Solver.PossibilityPosition;

public static class RestrictedPossibilityAlgorithms
{
    private static readonly GridPositions _buffer = new();
    
    public static bool ForeachSearch(IPossibilitiesPositions first, IPossibilitiesPositions second, int possibility)
    {
        foreach (var cell1 in first.EachCell(possibility))
        {
            foreach (var cell2 in second.EachCell(possibility))
            {
                if (!Cells.ShareAUnit(cell1, cell2)) return false;
            }
        }

        return true;
    }

    public static bool GridPositionsSearch(GridPositions first, GridPositions second, IPossibilitiesHolder holder, int possibility)
    {
        _buffer.Void();
        _buffer.ApplyOr(first);
        _buffer.ApplyOr(second);
        _buffer.ApplyAnd(holder.PositionsFor(possibility));

        return _buffer.CanBeCoverByAUnit();
    }
}