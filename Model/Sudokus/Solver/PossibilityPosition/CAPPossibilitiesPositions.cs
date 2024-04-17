using System;
using System.Collections.Generic;
using System.Text;
using Model.Helpers;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.StrategiesUtility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.PossibilityPosition;

public class CAPPossibilitiesPositions : IPossibilitiesPositions
{
    private readonly Cell[] _cells;
    private readonly ISudokuSolvingState _snapshot;
    private GridPositions? _gp;
    
    public ReadOnlyBitSet16 Possibilities { get; }
    public int PossibilityCount => Possibilities.Count;
    public int PositionsCount => _cells.Length;

    public CAPPossibilitiesPositions(Cell[] cells, ReadOnlyBitSet16 possibilities, ISudokuSolvingState snapshot)
    {
        _cells = cells;
        Possibilities = possibilities;
        _snapshot = snapshot;
    }
    
    public CAPPossibilitiesPositions(Cell cell, ReadOnlyBitSet16 possibilities, ISudokuSolvingState snapshot)
    {
        _cells = new[] { cell };
        Possibilities = possibilities;
        _snapshot = snapshot;
    }
    
    public IEnumerable<int> EachPossibility()
    {
        return Possibilities.EnumeratePossibilities();
    }

    public IEnumerable<Cell> EachCell()
    {
        return _cells;
    }

    public IEnumerable<Cell> EachCell(int possibility)
    {
        foreach(var cell in _cells)
        {
            if (_snapshot.PossibilitiesAt(cell.Row, cell.Column).Contains(possibility)) yield return cell;
        }
    }

    public ReadOnlyBitSet16 PossibilitiesInCell(Cell cell)
    {
        return Possibilities & _snapshot.PossibilitiesAt(cell);
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
            result[i] = new CellPossibilities(_cells[i], _snapshot.PossibilitiesAt(_cells[i]) & Possibilities);
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

        foreach (var pos in Possibilities.EnumeratePossibilities())
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