﻿using System;
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
        _pos = new LinePositions(cols);
    }

    public PointingRow(int possibility, int row, IEnumerable<int> cols)
    {
        Possibility = possibility;
        Row = row;
        _pos = new LinePositions(cols);
    }

    public PointingRow(int possibility, IReadOnlyList<CellPossibility> coords)
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

    public MinMax FindMinMaxColumns()
    {
        var minCol = 9;
        var maxCol = -1;
        foreach (var cell in EnumerateCells())
        {
            if (cell.Column < minCol) minCol = cell.Column;
            if (cell.Column > maxCol) maxCol = cell.Column;
        }

        return new MinMax(minCol, maxCol);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Possibility, Row, _pos.GetHashCode());
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PointingRow pr) return false;
        return pr.Possibility == Possibility && pr.Row == Row && pr._pos.Equals(_pos);
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

    public IEnumerable<int> EveryColumn() => _pos;

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

    public IEnumerable<int> EnumeratePossibilities()
    {
        yield return Possibility;
    }

    public IEnumerable<CellPossibilities> EnumerateCellPossibilities()
    {
        foreach (var col in _pos)
        {
            yield return new CellPossibilities(new Cell(Row, col), Possibility);
        }
    }

    public IEnumerable<Cell> EnumerateCells()
    {
        foreach (var col in _pos)
        {
            yield return new Cell(Row, col);
        }
    }

    public IEnumerable<CellPossibility> EnumerateCellPossibility()
    {
        foreach (var col in _pos)
        {
            yield return new CellPossibility(Row, col, Possibility);
        }
    }

    public bool Contains(Cell cell)
    {
        return Row == cell.Row && _pos.Contains(cell.Column);
    }

    public bool Contains(CellPossibility cp)
    {
        return Possibility == cp.Possibility && Row == cp.Row && _pos.Contains(cp.Column);
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