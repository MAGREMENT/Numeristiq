using System;
using System.Collections.Generic;
using System.Linq;
using Model.Solver.Positions;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.StrategiesUtil;

public class PointingRow : ILinkGraphElement
{
    public int Possibility { get; }
    public int Row { get; }
    private readonly LinePositions _pos;

    public int Count => _pos.Count;

    public PointingRow(int possibility, int row, LinePositions colPositions)
    {
        Possibility = possibility;
        Row = row;
        _pos = colPositions;
    }

    public PointingRow(int possibility, int row, params int[] cols)
    {
        Possibility = possibility;
        Row = row;
        _pos = new LinePositions();
        foreach(var col in cols)
        {
            _pos.Add(col);
        }
    }

    public PointingRow(int possibility, IEnumerable<CellPossibility> coords)
    {
        Possibility = possibility;
        Row = coords.First().Row;
        _pos = new LinePositions();
        foreach (var coord in coords)
        {
            if (coord.Row != Row) throw new ArgumentException("Not on same row");
            _pos.Add(coord.Col);
        }
    }

    public override int GetHashCode()
    {
        int coordsHash = 0;
        foreach (var col in _pos)
        {
            coordsHash ^= col.GetHashCode();
        }

        return HashCode.Combine(Possibility, Row, coordsHash);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PointingRow pr) return false;
        if (pr.Possibility != Possibility || pr.Row != Row || _pos.Count != pr._pos.Count) return false;
        foreach (var col in _pos)
        {
            if (!pr._pos.Peek(col)) return false;
        }

        return true;
    }

    public override string ToString()
    {
        var result = $"PR : {Possibility}";
        foreach (var col in _pos)
        {
            result += $"[{Row + 1}, {col + 1}], ";
        }
        
        return result[..^2];
    }

    public CellPossibilities[] EveryCellPossibilities()
    {
        CellPossibilities[] result = new CellPossibilities[_pos.Count];
        
        int cursor = 0;
        foreach (var col in _pos)
        {
            result[cursor] = new CellPossibilities(new Cell(Row, col), Possibility);
            cursor++;
        }

        return result;
    }

    public Cell[] EveryCell()
    {
        Cell[] result = new Cell[_pos.Count];
        
        int cursor = 0;
        foreach (var col in _pos)
        {
            result[cursor] = new Cell(Row, col);
            cursor++;
        }

        return result;
    }

    public IPossibilities EveryPossibilities()
    {
        var result = IPossibilities.NewEmpty();
        result.Add(Possibility);
        return result;
    }
}