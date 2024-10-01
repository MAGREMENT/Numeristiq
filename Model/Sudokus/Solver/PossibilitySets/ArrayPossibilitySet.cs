using System.Collections.Generic;
using System.Linq;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.PossibilitySets;

public class ArrayPossibilitySet : IPossibilitySet
{
    private readonly CellPossibilities[] _cps;

    public ArrayPossibilitySet(params CellPossibilities[] cps)
    {
        _cps = cps;
    }

    public IEnumerable<CellPossibilities> EnumerateCellPossibilities() => _cps;

    public IEnumerable<Cell> EnumerateCells()
    {
        foreach (var cp in _cps)
        {
            yield return cp.Cell;
        }
    }

    public IEnumerable<CellPossibility> EnumerateCellPossibility()
    {
        foreach (var cp in _cps)
        {
            foreach (var p in cp.Possibilities.EnumeratePossibilities())
            {
                yield return new CellPossibility(cp.Cell, p);
            }
        }
    }

    public bool Contains(Cell cell)
    {
        foreach (var cp in _cps)
        {
            if (cp.Cell == cell) return true;
        }

        return false;
    }

    public bool Contains(CellPossibility cellPossibility)
    {
        foreach (var cp in _cps)
        {
            if (cp.Cell != cellPossibility.ToCell()) continue;
            
            foreach (var p in cp.Possibilities.EnumeratePossibilities())
            {
                if (p == cellPossibility.Possibility) return true;
            }
        }

        return false;
    }

    public bool Contains(int possibility)
    {
        foreach (var cp in _cps)
        {
            foreach (var p in cp.Possibilities.EnumeratePossibilities())
            {
                if (p == possibility) return true;
            }
        }

        return false;
    }

    public bool Contains(CellPossibilities cp)
    {
        return _cps.Contains(cp);
    }

    public IEnumerable<Cell> EnumerateCells(int possibility)
    {
        foreach (var cp in _cps)
        {
            if (cp.Possibilities.Contains(possibility)) yield return cp.Cell;
        }
    }

    public ReadOnlyBitSet16 EveryPossibilities()
    {
        ReadOnlyBitSet16 result = new();
        foreach (var cp in _cps) result |= cp.Possibilities;

        return result;
    }

    public CellPossibilities[] EveryCellPossibilities()
    {
        return _cps;
    }

    public Cell[] EveryCell()
    {
        var result = new Cell[_cps.Length];
        for (int i = 0; i < _cps.Length; i++)
        {
            result[i] = _cps[i].Cell;
        }

        return result;
    }

    public CellPossibility[] EveryCellPossibility()
    {
        return EnumerateCellPossibility().ToArray();
    }

    public IEnumerable<int> EnumeratePossibilities()
    {
        return EveryPossibilities().EnumeratePossibilities();
    }

    public ReadOnlyBitSet16 PossibilitiesInCell(Cell cell)
    {
        foreach (var cp in _cps)
        {
            if (cp.Cell == cell) return cp.Possibilities;
        }

        return new ReadOnlyBitSet16();
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

    public bool IsPossibilityRestricted(IPossibilitySet other, int possibility)
    {
        return RestrictedPossibilityAlgorithms.AlternatingCommonHouseSearch(this, other, possibility);
    }

    public GridPositions PositionsFor(int p)
    {
        var gp = new GridPositions();

        foreach (var cp in _cps)
        {
            if(cp.Possibilities.Contains(p)) gp.Add(cp.Cell);
        }

        return gp;
    }

    public int PossibilityCount => EveryPossibilities().Count;
    public int PositionsCount => _cps.Length;

    public override bool Equals(object? obj)
    {
        return IPossibilitySet.InternalEquals(this, obj);
    }

    public override int GetHashCode()
    {
        return IPossibilitySet.InternalHash(this);
    }

    public override string ToString()
    {
        return IPossibilitySet.InternalToString(this);
    }
}