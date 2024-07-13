using System.Collections.Generic;
using System.Text;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.PossibilityPosition;

public class CPPossibilitiesPositions : IPossibilitiesPositions
{
    private readonly CellPossibilities[] _cps;

    public CPPossibilitiesPositions(CellPossibilities[] cps)
    {
        _cps = cps;
    }

    public IEnumerable<Cell> EnumerateCells()
    {
        foreach (var cp in _cps)
        {
            yield return cp.Cell;
        }
    }

    public IEnumerable<Cell> EnumerateCells(int possibility)
    {
        foreach (var cp in _cps)
        {
            if (cp.Possibilities.Contains(possibility)) yield return cp.Cell;
        }
    }

    public IEnumerable<CellPossibility> EnumeratePossibilities()
    {
        foreach (var cp in _cps)
        {
            foreach (var p in cp.Possibilities.EnumeratePossibilities())
            {
                yield return new CellPossibility(cp.Cell, p); 
            }
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

    public GridPositions PositionsFor(int p)
    {
        return new GridPositions(); //TODO
    }

    public int PossibilityCount => Possibilities.Count;
    public int PositionsCount => _cps.Length;
    
    public override string ToString()
    {
        var builder = new StringBuilder();

        foreach (var pos in Possibilities.EnumeratePossibilities())
        {
            builder.Append(pos);
        }

        builder.Append("{ ");
        foreach(var cell in EnumerateCells())
        {
            builder.Append(cell + " ");
        }

        builder.Append('}');

        return builder.ToString();
    }
}