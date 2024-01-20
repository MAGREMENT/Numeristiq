using System;
using System.Linq;
using System.Text;
using Global;
using Model.Solver.Possibility;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.StrategiesUtility;

public class CellsPossibility : IChainingElement
{
    private readonly int _possibility;
    private readonly Cell[] _cells;

    public CellsPossibility(int possibility, Cell[] cells)
    {
        _possibility = possibility;
        _cells = cells;
    }

    public int Rank => 2;
    
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

    public Possibilities EveryPossibilities()
    {
        var poss = Possibilities.NewEmpty();
        poss.Add(_possibility);
        return poss;
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