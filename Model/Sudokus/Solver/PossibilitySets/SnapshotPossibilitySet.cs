using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.PossibilitySets;

public class SnapshotPossibilitySet : IPossibilitySet
{
    private readonly ReadOnlyBitSet16 _possibilities;
    private readonly Cell[] _cells;
    private GridPositions? _gp;
    private readonly ISudokuSolvingState _snapshot;

    public SnapshotPossibilitySet(Cell[] cells, ReadOnlyBitSet16 possibilities, ISudokuSolvingState snapshot)
    {
        _cells = cells;
        _possibilities = possibilities;
        _snapshot = snapshot;
    }
    
    public SnapshotPossibilitySet(Cell cell, ReadOnlyBitSet16 possibilities, ISudokuSolvingState snapshot)
    {
        _cells = new[] { cell };
        _possibilities = possibilities;
        _snapshot = snapshot;
    }
    
    public GridPositions PositionsFor(int p)
    {
        return Positions.And(_snapshot.PositionsFor(p));
    }

    public int PossibilityCount => _possibilities.Count;
    public int PositionsCount => _cells.Length;

    public IEnumerable<CellPossibilities> EnumerateCellPossibilities()
    {
        foreach (var cell in _cells)
        {
            yield return new CellPossibilities(cell, _snapshot.PossibilitiesAt(cell) & _possibilities);
        }
    }

    public IEnumerable<Cell> EnumerateCells()
    {
        return _cells;
    }

    public IEnumerable<CellPossibility> EnumerateCellPossibility()
    {
        foreach (var cell in _cells)
        {
            foreach (var p in _possibilities.EnumeratePossibilities())
            {
                if (_snapshot.PossibilitiesAt(cell).Contains(p)) yield return new CellPossibility(cell, p);
            }
        }
    }

    public bool Contains(Cell cell)
    {
        return _cells.Contains(cell);
    }

    public bool Contains(CellPossibility cp)
    {
        var cell = cp.ToCell();
        return Contains(cell) & _possibilities.Contains(cp.Possibility) 
                              & _snapshot.PossibilitiesAt(cell).Contains(cp.Possibility);
    }

    public bool Contains(int possibility)
    {
        return _possibilities.Contains(possibility);
    }

    public bool Contains(CellPossibilities cp)
    {
        return _cells.Contains(cp.Cell) && (_snapshot.PossibilitiesAt(cp.Cell) & _possibilities) == cp.Possibilities;
    }

    public IEnumerable<Cell> EnumerateCells(int possibility)
    {
        foreach(var cell in _cells)
        {
            if (_snapshot.PossibilitiesAt(cell.Row, cell.Column).Contains(possibility)) yield return cell;
        }
    }

    public ReadOnlyBitSet16 EveryPossibilities() => _possibilities;

    public CellPossibilities[] EveryCellPossibilities()
    {
        var result = new CellPossibilities[PositionsCount];
        for (int i = 0; i < _cells.Length; i++)
        {
            result[i] = new CellPossibilities(_cells[i], _snapshot.PossibilitiesAt(_cells[i]) & _possibilities);
        }

        return result;
    }

    public Cell[] EveryCell() => _cells;

    public CellPossibility[] EveryCellPossibility()
    {
        return EnumerateCellPossibility().ToArray();
    }

    public IEnumerable<int> EnumeratePossibilities() => _possibilities.EnumeratePossibilities();

    public ReadOnlyBitSet16 PossibilitiesInCell(Cell cell)
    {
        return _possibilities & _snapshot.PossibilitiesAt(cell);
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

    public bool IsPossibilityRestricted(IPossibilitySet other, int possibility)
    {
        return RestrictedPossibilityAlgorithms.AlternatingCommonHouseSearch(this, other, possibility);
    }

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