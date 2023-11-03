using System.Collections.Generic;
using System.Linq;
using Model.Solver.Positions;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.PossibilitiesPositions;

public class CellPossibilitiesPossibilitiesPositions : IPossibilitiesPositions
{
    private readonly CellPossibilities[] _cps;

    public CellPossibilitiesPossibilitiesPositions(CellPossibilities[] cps)
    {
        _cps = cps;
    }

    public IEnumerable<int> EachPossibility()
    {
        return Possibilities;
    }

    public IEnumerable<Cell> EachCell()
    {
        foreach (var cp in _cps)
        {
            yield return cp.Cell;
        }
    }

    public IEnumerable<Cell> EachCellWithPossibility(int possibility)
    {
        foreach (var cp in _cps)
        {
            if (cp.Possibilities.Peek(possibility)) yield return cp.Cell;
        }
    }

    public IEnumerable<int> EachPossibilityWithCell(Cell cell)
    {
        foreach (var cp in _cps)
        {
            if (cp.Cell == cell) return cp.Possibilities;
        }
        
        return Enumerable.Empty<int>();
    }

    public IPossibilities Possibilities
    {
        get
        {
            IPossibilities result = IPossibilities.NewEmpty();
            foreach (var cp in _cps)
            {
                result.Add(cp.Possibilities);
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

    public int PossibilityCount => Possibilities.Count;
    public int PositionsCount => _cps.Length;
}