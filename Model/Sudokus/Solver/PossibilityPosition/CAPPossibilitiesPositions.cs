using System;
using System.Collections.Generic;
using System.Text;
using Model.Core;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.PossibilityPosition;

public class CAPPossibilitiesPositions : IPossibilitiesPositions
{
    private readonly Cell[] _cells;
    private GridPositions? _gp;
    
    public ReadOnlyBitSet16 Possibilities { get; }
    public GridPositions PositionsFor(int p)
    {
        return Positions.And(Snapshot.PositionsFor(p));
    }

    public int PossibilityCount => Possibilities.Count;
    public int PositionsCount => _cells.Length;
    public ISudokuSolvingState Snapshot { get; }

    public CAPPossibilitiesPositions(Cell[] cells, ReadOnlyBitSet16 possibilities, ISudokuSolvingState snapshot)
    {
        _cells = cells;
        Possibilities = possibilities;
        Snapshot = snapshot;
    }
    
    public CAPPossibilitiesPositions(Cell cell, ReadOnlyBitSet16 possibilities, ISudokuSolvingState snapshot)
    {
        _cells = new[] { cell };
        Possibilities = possibilities;
        Snapshot = snapshot;
    }

    public IEnumerable<Cell> EnumerateCells()
    {
        return _cells;
    }

    public IEnumerable<Cell> EnumerateCells(int possibility)
    {
        foreach(var cell in _cells)
        {
            if (Snapshot.PossibilitiesAt(cell.Row, cell.Column).Contains(possibility)) yield return cell;
        }
    }

    public IEnumerable<CellPossibility> EnumeratePossibilities()
    {
        foreach (var cell in _cells)
        {
            foreach (var p in Possibilities.EnumeratePossibilities())
            {
                if (Snapshot.PossibilitiesAt(cell).Contains(p)) yield return new CellPossibility(cell, p);
            }
        }
    }

    public ReadOnlyBitSet16 PossibilitiesInCell(Cell cell)
    {
        return Possibilities & Snapshot.PossibilitiesAt(cell);
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
            result[i] = new CellPossibilities(_cells[i], Snapshot.PossibilitiesAt(_cells[i]) & Possibilities);
        }

        return result;
    }

    public bool IsPossibilityRestricted(IPossibilitiesPositions other, int possibility)
    {
        return RestrictedPossibilityAlgorithms.ForeachSearch(this, other, possibility);
        //return RestrictedPossibilityAlgorithms.GridPositionsSearch(Positions, other.Positions, Snapshot, possibility);
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
}