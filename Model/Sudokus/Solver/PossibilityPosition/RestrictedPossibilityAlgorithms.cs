using Model.Core;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.Utility;
using Model.Utility;

namespace Model.Sudokus.Solver.PossibilityPosition;

public static class RestrictedPossibilityAlgorithms
{
    public static bool ForeachSearch(IPossibilitiesPositions first, IPossibilitiesPositions second, int possibility)
    {
        foreach (var cell1 in first.EnumerateCells(possibility))
        {
            foreach (var cell2 in second.EnumerateCells(possibility))
            {
                if (!SudokuCellUtility.ShareAUnit(cell1, cell2)) return false;
            }
        }

        return true;
    }

    public static bool GridPositionsSearch(IPossibilitiesPositions first, IPossibilitiesPositions second,
        int possibility)
    {
        var result = first.PositionsFor(possibility);
        result.ApplyOr(second.PositionsFor(possibility));
        return result.CanBeCoveredByAUnit();
    }

    public static bool CellEnumerationSearch(IPossibilitiesPositions first, IPossibilitiesPositions second,
        int possibility)
    {
        SharedHouses? sh = null;

        foreach (var cell in first.EnumerateCells(possibility))
        {
            if (sh is null) sh = new SharedHouses(cell);
            else
            {
                sh.Share(cell);
                if (sh.Count == 0) return false;
            }
        }

        foreach (var cell in second.EnumerateCells(possibility))
        {
            sh!.Share(cell);
            if (sh.Count == 0) return false;
        }

        return true;
    }
}