using System;
using System.Collections.Generic;
using System.Text;
using Model.Sudoku.Solver.Position;
using Model.Sudoku.Solver.Possibility;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;

namespace Model.Sudoku.Solver.PossibilityPosition;

public class CAPPossibilitiesPositions : IPossibilitiesPositions
{
    private readonly Cell[] _cells;
    private readonly IPossibilitiesHolder _snapshot;
    private GridPositions? _gp;
    
    public IReadOnlyPossibilities Possibilities { get; }
    public int PossibilityCount => Possibilities.Count;
    public int PositionsCount => _cells.Length;

    public CAPPossibilitiesPositions(Cell[] cells, IReadOnlyPossibilities possibilities, IPossibilitiesHolder snapshot)
    {
        _cells = cells;
        Possibilities = possibilities;
        _snapshot = snapshot;
    }
    
    public CAPPossibilitiesPositions(Cell cell, IReadOnlyPossibilities possibilities, IPossibilitiesHolder snapshot)
    {
        _cells = new[] { cell };
        Possibilities = possibilities;
        _snapshot = snapshot;
    }
    
    public IEnumerable<int> EachPossibility()
    {
        return Possibilities;
    }

    public IEnumerable<Cell> EachCell()
    {
        return _cells;
    }

    public IEnumerable<Cell> EachCell(int possibility)
    {
        foreach(var cell in _cells)
        {
            if (_snapshot.PossibilitiesAt(cell.Row, cell.Column).Peek(possibility)) yield return cell;
        }
    }

    public IReadOnlyPossibilities PossibilitiesInCell(Cell cell)
    {
        return Possibilities.And(_snapshot.PossibilitiesAt(cell));
    }

    public GridPositions Positions
    {
        get
        {
            if (_gp is null)
            {
                _gp = new GridPositions();
                foreach (var cell in _cells)
                {
                    _gp.Add(cell);
                }
            }

            return _gp;
        }
    }

    public CellPossibilities[] ToCellPossibilitiesArray()
    {
        CellPossibilities[] result = new CellPossibilities[PositionsCount];
        for (int i = 0; i < _cells.Length; i++)
        {
            result[i] = new CellPossibilities(_cells[i], _snapshot.PossibilitiesAt(_cells[i]).And(Possibilities));
        }

        return result;
    }

    public bool IsPossibilityRestricted(IPossibilitiesPositions other, int possibility)
    {
        //return RestrictedPossibilityAlgorithms.ForeachSearch(this, other, possibility);
        return RestrictedPossibilityAlgorithms.GridPositionsSearch(Positions, other.Positions, _snapshot, possibility);
    }

    public override bool Equals(object? obj)
    {
        return obj is CAPPossibilitiesPositions pp && Possibilities.Equals(pp.Possibilities)
                                                                     && Positions.Equals(pp.Positions);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Possibilities.GetHashCode(), Positions.GetHashCode());
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        foreach (var pos in Possibilities)
        {
            builder.Append(pos);
        }

        builder.Append("{ ");
        foreach(var cell in EachCell())
        {
            builder.Append(cell + " ");
        }

        builder.Append('}');

        return builder.ToString();
    }
}