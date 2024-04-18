using System;
using System.Collections.Generic;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility;

public class PointingRow : ISudokuElement
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

    public PointingRow(int possibility, List<CellPossibility> coords)
    {
        Possibility = possibility;
        Row = coords[0].Row;
        _pos = new LinePositions();
        for (int i = 1; i < coords.Count; i++)
        {
            if (coords[i].Row != Row) throw new ArgumentException("Not on same row");
            _pos.Add(coords[i].Column);
        }
        
        _pos.Add(coords[0].Column);
    }
    
    public PointingRow(int possibility, List<Cell> coords)
    {
        Possibility = possibility;
        Row = coords[0].Row;
        _pos = new LinePositions();
        for (int i = 1; i < coords.Count; i++)
        {
            if (coords[i].Row != Row) throw new ArgumentException("Not on same row");
            _pos.Add(coords[i].Column);
        }
        
        _pos.Add(coords[0].Column);
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
            if (!pr._pos.Contains(col)) return false;
        }

        return true;
    }

    public override string ToString()
    {
        var result = $"{Possibility}r{Row + 1}c";
        foreach (var col in _pos)
        {
            result += $"{col + 1}";
        }
        
        return result;
    }

    public int DifficultyRank => 2;

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

    public ReadOnlyBitSet16 EveryPossibilities()
    {
        return new ReadOnlyBitSet16(Possibility);
    }

    public CellPossibility[] EveryCellPossibility()
    {
        CellPossibility[] result = new CellPossibility[_pos.Count];
        
        int cursor = 0;
        foreach (var col in _pos)
        {
            result[cursor] = new CellPossibility(Row, col, Possibility);
            cursor++;
        }

        return result;
    }
    
    public bool Contains(Cell cell)
    {
        return Row == cell.Row && _pos.Contains(cell.Column);
    }

    public bool Contains(CellPossibility cp)
    {
        return Possibility == cp.Possibility && Row == cp.Row && _pos.Contains(cp.Column);
    }
}