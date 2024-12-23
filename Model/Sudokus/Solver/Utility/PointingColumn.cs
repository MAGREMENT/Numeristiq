﻿using System;
using System.Collections.Generic;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility;

public class PointingColumn : ISudokuElement
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
        _pos = new LinePositions(rows);
    }
    
    public PointingColumn(int possibility, int column, IEnumerable<int> rows)
    {
        Possibility = possibility;
        Column = column;
        _pos = new LinePositions(rows);
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

    public MinMax FindMinMaxRows()
    {
        var minRow = 9;
        var maxRow = -1;
        foreach (var cell in EnumerateCells())
        {
            if (cell.Row < minRow) minRow = cell.Row;
            if (cell.Row > maxRow) maxRow = cell.Row;
        }

        return new MinMax(minRow, maxRow);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Possibility, Column, _pos.GetHashCode());
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PointingColumn pc) return false;
        return pc.Possibility == Possibility && pc.Column == Column && _pos.Equals(pc._pos);
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

    public int DifficultyRank => 2;

    public IEnumerable<int> EveryRow() => _pos;

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

    public ReadOnlyBitSet16 EveryPossibilities()
    {
        return new ReadOnlyBitSet16(Possibility);
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

    public IEnumerable<int> EnumeratePossibilities()
    {
        yield return Possibility;
    }

    public IEnumerable<CellPossibilities> EnumerateCellPossibilities()
    {
        foreach (var row in _pos)
        {
            yield return new CellPossibilities(new Cell(row, Column), Possibility);
        }
    }

    public IEnumerable<Cell> EnumerateCells()
    {
        foreach (var row in _pos)
        {
            yield return new Cell(row, Column);
        }
    }

    public IEnumerable<CellPossibility> EnumerateCellPossibility()
    {
        foreach (var row in _pos)
        {
            yield return new CellPossibility(row, Column, Possibility);
        }
    }

    public bool Contains(Cell cell)
    {
        return Column == cell.Column && _pos.Contains(cell.Row);
    }

    public bool Contains(CellPossibility cp)
    {
        return Possibility == cp.Possibility && Column == cp.Column && _pos.Contains(cp.Row);
    }

    public bool Contains(int possibility)
    {
        return Possibility == possibility;
    }
    
    public bool Contains(CellPossibilities cp)
    {
        return cp.Possibilities.Count == 1 && Contains(cp.Possibilities.FirstPossibility()) && Contains(cp.Cell);
    }
}