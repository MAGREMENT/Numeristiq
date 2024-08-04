using System;
using System.Collections.Generic;
using System.Text;
using Model.Core;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.PossibilitySets;

public class SnapshotPossibilitySet : IPossibilitySet
{
    private readonly Cell[] _cells;
    private GridPositions? _gp;
    private readonly ISudokuSolvingState _snapshot;
    
    public ReadOnlyBitSet16 Possibilities { get; }
    public GridPositions PositionsFor(int p)
    {
        return Positions.And(_snapshot.PositionsFor(p));
    }

    public int PossibilityCount => Possibilities.Count;
    public int PositionsCount => _cells.Length;

    public SnapshotPossibilitySet(Cell[] cells, ReadOnlyBitSet16 possibilities, ISudokuSolvingState snapshot)
    {
        _cells = cells;
        Possibilities = possibilities;
        _snapshot = snapshot;
    }
    
    public SnapshotPossibilitySet(Cell cell, ReadOnlyBitSet16 possibilities, ISudokuSolvingState snapshot)
    {
        _cells = new[] { cell };
        Possibilities = possibilities;
        _snapshot = snapshot;
    }

    public IEnumerable<Cell> EnumerateCells()
    {
        return _cells;
    }

    public IEnumerable<Cell> EnumerateCells(int possibility)
    {
        foreach(var cell in _cells)
        {
            if (_snapshot.PossibilitiesAt(cell.Row, cell.Column).Contains(possibility)) yield return cell;
        }
    }

    public IEnumerable<CellPossibility> EnumeratePossibilities()
    {
        foreach (var cell in _cells)
        {
            foreach (var p in Possibilities.EnumeratePossibilities())
            {
                if (_snapshot.PossibilitiesAt(cell).Contains(p)) yield return new CellPossibility(cell, p);
            }
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

    public bool IsPossibilityRestricted(IPossibilitySet other, int possibility)
    {
        return RestrictedPossibilityAlgorithms.AlternatingCommonHouseSearch(this, other, possibility);
    }

    public override bool Equals(object? obj)
    {
        return obj is SnapshotPossibilitySet pp && Possibilities.Equals(pp.Possibilities)
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
        foreach(var cell in EnumerateCells())
        {
            builder.Append(cell + " ");
        }

        builder.Append('}');

        return builder.ToString();
    }
}