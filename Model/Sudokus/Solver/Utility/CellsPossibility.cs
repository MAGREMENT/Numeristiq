using System;
using System.Linq;
using System.Text;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility;

public class CellsPossibility : ISudokuElement
{
    private readonly int _possibility;
    private readonly Cell[] _cells;

    public CellsPossibility(int possibility, Cell[] cells)
    {
        _possibility = possibility;
        _cells = cells;
    }

    public int DifficultyRank => 2;
    
    public CellPossibilities[] EveryCellPossibilities()
    {
        var result = new CellPossibilities[_cells.Length];
        for (int i = 0; i < _cells.Length; i++)
        {
            result[i] = new CellPossibilities(_cells[i], _possibility);
        }

        return result;
    }

    public Cell[] EveryCell()
    {
        return _cells;
    }

    public ReadOnlyBitSet16 EveryPossibilities()
    {
        return new ReadOnlyBitSet16(_possibility);
    }

    public CellPossibility[] EveryCellPossibility()
    {
        var result = new CellPossibility[_cells.Length];
        for (int i = 0; i < _cells.Length; i++)
        {
            result[i] = new CellPossibility(_cells[i], _possibility);
        }

        return result;
    }

    public bool Contains(Cell cell)
    {
        return _cells.Contains(cell);
    }

    public bool Contains(CellPossibility cp)
    {
        return _possibility == cp.Possibility && _cells.Contains(cp.ToCell());
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CellsPossibility cp || cp._possibility != _possibility 
            || cp._cells.Length != _cells.Length) return false;
        foreach (var c in cp._cells)
        {
            if (!_cells.Contains(c)) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hash = 0;
        foreach (var c in _cells)
        {
            hash ^= c.GetHashCode();
        }

        return HashCode.Combine(_possibility, hash);
    }

    public override string ToString()
    {
        var builder = new StringBuilder($"{_possibility}{_cells[0]}");

        for (int i = 1; i < _cells.Length; i++)
        {
            builder.Append($", {_cells[i]}");
        }

        return builder.ToString();
    }
}