using System.Collections.Generic;
using System.Linq;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility.AlmostLockedSets;

public class NakedSet : ISudokuElement
{
    private readonly CellPossibilities[] _cellPossibilities;
    
    public int DifficultyRank => 3;

    public NakedSet(CellPossibilities[] cellPossibilities)
    {
        _cellPossibilities = cellPossibilities;
    }

    
    public CellPossibilities[] EveryCellPossibilities()
    {
        return _cellPossibilities;
    }

    public Cell[] EveryCell()
    {
        Cell[] result = new Cell[_cellPossibilities.Length];
        for (int i = 0; i < _cellPossibilities.Length; i++)
        {
            result[i] = _cellPossibilities[i].Cell;
        }

        return result;
    }

    public ReadOnlyBitSet16 EveryPossibilities()
    {
        ReadOnlyBitSet16 result = new();

        foreach (var cp in _cellPossibilities)
        {
            result |= cp.Possibilities;
        }

        return result;
    }

    public CellPossibility[] EveryCellPossibility()
    {
        List<CellPossibility> result = new();
        foreach (var cp in _cellPossibilities)
        {
            foreach (var p in cp.Possibilities.EnumeratePossibilities())
            {
                result.Add(new CellPossibility(cp.Cell, p));
            }
        }

        return result.ToArray();
    }

    public IEnumerable<int> EnumeratePossibilities()
    {
        return EveryPossibilities().EnumeratePossibilities();
    }

    public IEnumerable<CellPossibilities> EnumerateCellPossibilities()
    {
        return _cellPossibilities;
    }

    public IEnumerable<Cell> EnumerateCell()
    {
        for (int i = 0; i < _cellPossibilities.Length; i++)
        {
            yield return _cellPossibilities[i].Cell;
        }
    }

    public IEnumerable<CellPossibility> EnumerateCellPossibility()
    {
        foreach (var cp in _cellPossibilities)
        {
            foreach (var p in cp.Possibilities.EnumeratePossibilities())
            {
                yield return new CellPossibility(cp.Cell, p);
            }
        }
    }

    public bool Contains(Cell cell)
    {
        foreach (var cp in _cellPossibilities)
        {
            if (cp.Cell == cell) return true;
        }

        return false;
    }

    public bool Contains(CellPossibility cp)
    {
        foreach (var cps in _cellPossibilities)
        {
            if (cps.Cell == cp.ToCell()) return cps.Possibilities.Contains(cp.Possibility);
        }

        return false;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not NakedSet np) return false;
        foreach (CellPossibilities cp in _cellPossibilities)
        {
            if (!np._cellPossibilities.Contains(cp)) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hashCode = 0;
        foreach (var coord in _cellPossibilities)
        {
            hashCode ^= coord.GetHashCode();
        }

        return hashCode;
    }

    public override string ToString()
    {
        var result = $"{EveryPossibilities().ToValuesString()}";
        foreach (var coord in _cellPossibilities)
        {
            result += $"{coord.Cell}, ";
        }

        return result[..^2];
    }
}