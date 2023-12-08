using System.Collections.Generic;
using System.Linq;
using Global;
using Model.Solver.Position;
using Model.Solver.Possibility;
using Model.Solver.StrategiesUtility;

namespace Model.Solver.PossibilityPosition;

public class CPPossibilitiesPositions : IPossibilitiesPositions
{
    private readonly CellPossibilities[] _cps;

    public CPPossibilitiesPositions(CellPossibilities[] cps)
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

    public IEnumerable<Cell> EachCell(int possibility)
    {
        foreach (var cp in _cps)
        {
            if (cp.Possibilities.Peek(possibility)) yield return cp.Cell;
        }
    }

    public IReadOnlyPossibilities PossibilitiesInCell(Cell cell)
    {
        foreach (var cp in _cps)
        {
            if (cp.Cell == cell) return cp.Possibilities;
        }

        return Possibility.Possibilities.NewEmpty();
    }

    public IReadOnlyPossibilities Possibilities
    {
        get
        {
            Possibilities result = Possibility.Possibilities.NewEmpty();
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

    public CellPossibilities[] ToCellPossibilitiesArray()
    {
        return _cps;
    }

    public int PossibilityCount => Possibilities.Count;
    public int PositionsCount => _cps.Length;
}