using System.Collections.Generic;
using Model.Sudoku.Solver.Position;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudoku.Solver.PossibilityPosition;

public class CPPossibilitiesPositions : IPossibilitiesPositions
{
    private readonly CellPossibilities[] _cps;

    public CPPossibilitiesPositions(CellPossibilities[] cps)
    {
        _cps = cps;
    }

    public IEnumerable<int> EachPossibility()
    {
        return Possibilities.EnumeratePossibilities();
    }

    public IEnumerable<Cell> EachCell()
    {
        foreach (var cp in _cps)
        {
            yield return cp.Cell;
        }
    }

    public IEnumerable<Cell> EachCell(int possibility)
    {
        foreach (var cp in _cps)
        {
            if (cp.Possibilities.Contains(possibility)) yield return cp.Cell;
        }
    }

    public ReadOnlyBitSet16 PossibilitiesInCell(Cell cell)
    {
        foreach (var cp in _cps)
        {
            if (cp.Cell == cell) return cp.Possibilities;
        }

        return new ReadOnlyBitSet16();
    }

    public ReadOnlyBitSet16 Possibilities
    {
        get
        {
            ReadOnlyBitSet16 result = new();
            foreach (var cp in _cps)
            {
                result += cp.Possibilities;
            }

            return result;
        }
    }

    public GridPositions Positions
    {
        get
        {
            GridPositions result = new GridPositions();
            foreach (var cp in _cps)
            {
                result.Add(cp.Cell);
            }

            return result;
        }
    }

    public CellPossibilities[] ToCellPossibilitiesArray()
    {
        return _cps;
    }

    public bool IsPossibilityRestricted(IPossibilitiesPositions other, int possibility)
    {
        return RestrictedPossibilityAlgorithms.ForeachSearch(this, other, possibility);
    }

    public int PossibilityCount => Possibilities.Count;
    public int PositionsCount => _cps.Length;
}