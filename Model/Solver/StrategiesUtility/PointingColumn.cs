using System;
using System.Collections.Generic;
using Global;
using Model.Solver.Position;
using Model.Solver.Possibility;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.StrategiesUtility;

public class PointingColumn : ILinkGraphElement
{
    public int Possibility { get; }
    public int Column { get; }
    private readonly LinePositions _pos;

    public int Count => _pos.Count;

    public PointingColumn(int possibility, int column, LinePositions rowPos)
    {
        Possibility = possibility;
        Column = column;
        _pos = rowPos;
    }

    public PointingColumn(int possibility, int column, params int[] rows)
    {
        Possibility = possibility;
        Column = column;
        _pos = new LinePositions();
        foreach (var row in rows)
        {
            _pos.Add(row);
        }
    }
    
    public PointingColumn(int possibility, List<CellPossibility> coords)
    {
        Possibility = possibility;
        Column = coords[0].Column;
        _pos = new LinePositions();
        for (int i = 1; i < coords.Count; i++)
        {
            if (coords[i].Column != Column) throw new ArgumentException("Not on same column");
            _pos.Add(coords[i].Row); 
        }
        
        _pos.Add(coords[0].Row);
    }
    
    public PointingColumn(int possibility, List<Cell> coords)
    {
        Possibility = possibility;
        Column = coords[0].Column;
        _pos = new LinePositions(); 
        for (int i = 1; i < coords.Count; i++)
        {
            if (coords[i].Column != Column) throw new ArgumentException("Not on same column");
            _pos.Add(coords[i].Row); 
        }

        _pos.Add(coords[0].Row);
    }

    public override int GetHashCode()
    {
        int coordsHash = 0;
        foreach (var row in _pos)
        {
            coordsHash ^= row;
        }

        return HashCode.Combine(Possibility, Column, coordsHash);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PointingColumn pc) return false;
        if (pc.Possibility != Possibility || pc.Column != Column || _pos.Count != pc._pos.Count) return false;
        foreach (var posRow in _pos)
        {
            if (!pc._pos.Peek(posRow)) return false;
        }

        return true;
    }
    
    public override string ToString()
    {
        var result = $"{Possibility}r";
        foreach (var row in _pos)
        {
            result += $"{row + 1}";
        }

        return result + $"c{Column + 1}";
    }

    public int Rank => 2;

    public CellPossibilities[] EveryCellPossibilities()
    {
        CellPossibilities[] result = new CellPossibilities[_pos.Count];
        
        int cursor = 0;
        foreach (var row in _pos)
        {
            result[cursor] = new CellPossibilities(new Cell(row, Column), Possibility);
            cursor++;
        }

        return result;
    }

    public Cell[] EveryCell()
    {
        Cell[] result = new Cell[_pos.Count];
        
        int cursor = 0;
        foreach (var row in _pos)
        {
            result[cursor] = new Cell(row, Column);
            cursor++;
        }

        return result;
    }

    public Possibilities EveryPossibilities()
    {
        var result = Possibilities.NewEmpty();
        result.Add(Possibility);
        return result;
    }

    public CellPossibility[] EveryCellPossibility()
    {
        CellPossibility[] result = new CellPossibility[_pos.Count];
        
        int cursor = 0;
        foreach (var row in _pos)
        {
            result[cursor] = new CellPossibility(row, Column, Possibility);
            cursor++;
        }

        return result;
    }
}