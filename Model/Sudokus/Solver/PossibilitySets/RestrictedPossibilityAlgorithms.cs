using Model.Sudokus.Solver.Utility;

namespace Model.Sudokus.Solver.PossibilitySets;

public static class RestrictedPossibilityAlgorithms
{
    public static bool EachCaseSearch(IPossibilitySet first, IPossibilitySet second, int possibility)
    {
        foreach (var cell1 in first.EnumerateCells(possibility))
        {
            foreach (var cell2 in second.EnumerateCells(possibility))
            {
                if (!SudokuUtility.ShareAUnit(cell1, cell2)) return false;
            }
        }

        return true;
    }

    public static bool GridPositionsSearch(IPossibilitySet first, IPossibilitySet second,
        int possibility)
    {
        var result = first.PositionsFor(possibility);
        result.ApplyOr(second.PositionsFor(possibility));
        return result.CanBeCoveredByAUnit();
    }

    public static bool CommonHouseSearch(IPossibilitySet first, IPossibilitySet second,
        int possibility)
    {
        CommonHouses ch = new();

        foreach (var cell in first.EnumerateCells(possibility))
        {
            ch = ch.Adapt(cell);
        }

        foreach (var cell in second.EnumerateCells(possibility))
        {
            ch = ch.Adapt(cell);
            if (!ch.IsValid()) return false;
        }

        return true;
    }
    
    public static bool AlternatingCommonHouseSearch(IPossibilitySet first, IPossibilitySet second,
        int possibility)
    {
        CommonHouses ch = new();
        using var enum1 = first.EnumerateCells(possibility).GetEnumerator();
        using var enum2 = second.EnumerateCells(possibility).GetEnumerator();

        var n1 = enum1.MoveNext();
        var n2 = enum2.MoveNext();
        while (n1 && n2)
        {
            ch = ch.Adapt(enum1.Current);
            ch = ch.Adapt(enum2.Current);

            if (!ch.IsValid()) return false;

            n1 = enum1.MoveNext();
            n2 = enum2.MoveNext();
        }

        while (n1)
        {
            ch = ch.Adapt(enum1.Current);
            if (!ch.IsValid()) return false;

            n1 = enum1.MoveNext();
        }

        while (n2)
        {
            ch = ch.Adapt(enum2.Current);
            if (!ch.IsValid()) return false;
            
            n2 = enum2.MoveNext();
        }

        return true;
    }
}