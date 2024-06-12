using System;
using System.Collections.Generic;
using System.Text;
using Model.Utility;

namespace Model.Tectonics.Solver.Utility;

public class ZoneGroup : ITectonicElement
{
    public int Possibility { get; }
    
    private readonly List<Cell> _cells;

    public IReadOnlyList<Cell> Cells => _cells;

    public ZoneGroup(List<Cell> cells, int possibility)
    {
        _cells = cells;
        Possibility = possibility;
    }

    public IEnumerable<Cell> EnumerateCell()
    {
        return _cells;
    }

    public IEnumerable<CellPossibility> EnumerateCellPossibility()
    {
        foreach (var cell in _cells)
        {
            yield return new CellPossibility(cell, Possibility);
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is not ZoneGroup zg || zg._cells.Count != _cells.Count
                                    || zg.Possibility != Possibility) return false;

        foreach (var cell in _cells)
        {
            if (!zg._cells.Contains(cell)) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hash = 0;
        foreach (var cell in _cells)
        {
            hash ^= cell.GetHashCode();
        }

        return HashCode.Combine(Possibility, hash);
    }

    public override string ToString()
    {
        var builder = new StringBuilder($"{Possibility}{_cells[0]}");

        for (int i = 1; i < _cells.Count; i++)
        {
            builder.Append($", {_cells[i]}");
        }

        return builder.ToString();
    }
}