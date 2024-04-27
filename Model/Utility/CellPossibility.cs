using System;
using System.Collections.Generic;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Tectonics.Utility;
using Model.Utility.BitSets;

namespace Model.Utility;

public readonly struct CellPossibility : ISudokuElement, ITectonicElement
{
    public int Possibility { get; }
    public int Row { get; }
    public int Column { get; }

    public CellPossibility(int row, int col, int possibility)
    {
        Possibility = possibility;
        Row = row;
        Column = col;
    }

    public CellPossibility(Cell coord, int possibility)
    {
        Possibility = possibility;
        Row = coord.Row;
        Column = coord.Column;
    }
    
    public bool ShareAUnit(CellPossibility coord)
    {
        return SudokuCellUtility.ShareAUnit(Row, Column, coord.Row, coord.Column);
    }
    
    public bool ShareAUnit(Cell coord)
    {
        return SudokuCellUtility.ShareAUnit(Row, Column, coord.Row, coord.Column);
    }

    public IEnumerable<Cell> SharedSeenCells(CellPossibility coord)
    {
        return SudokuCellUtility.SharedSeenCells(Row, Column, coord.Row, coord.Column);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Possibility, Row, Column);
    }

    public override bool Equals(object? obj)
    {
        return obj is CellPossibility cp && cp == this;
    }

    public override string ToString()
    {
        return $"{Possibility}r{Row + 1}c{Column + 1}";
    }

    public int DifficultyRank => 1;

    public CellPossibilities[] EveryCellPossibilities()
    {
        return new[] { new CellPossibilities(this) };
    }

    public Cell[] EveryCell()
    {
        return new Cell[] { new(Row, Column) };
    }

    public ReadOnlyBitSet16 EveryPossibilities()
    {
        var result = new ReadOnlyBitSet16();
        result += Possibility;
        return result;
    }

    public CellPossibility[] EveryCellPossibility()
    {
        return new[] { this };
    }

    public IEnumerable<int> EnumeratePossibilities()
    {
        yield return Possibility;
    }

    public IEnumerable<CellPossibilities> EnumerateCellPossibilities()
    {
        yield return new CellPossibilities(new Cell(Row, Column), Possibility);
    }

    public IEnumerable<Cell> EnumerateCell()
    {
        yield return new Cell(Row, Column);
    }

    public IEnumerable<CellPossibility> EnumerateCellPossibility()
    {
        yield return new CellPossibility(new Cell(Row, Column), Possibility);
    }

    public bool Contains(Cell cell)
    {
        return cell.Row == Row && cell.Column == Column;
    }

    public bool Contains(CellPossibility cp)
    {
        return cp == this;
    }

    public Cell ToCell()
    {
        return new Cell(Row, Column);
    }

    public static bool operator ==(CellPossibility left, CellPossibility right)
    {
        return left.Possibility == right.Possibility && left.Row == right.Row && left.Column == right.Column;
    }

    public static bool operator !=(CellPossibility left, CellPossibility right)
    {
        return !(left == right);
    }
}

public class CellPossibilities
{
    public Cell Cell { get; }
    public ReadOnlyBitSet16 Possibilities { get; }
    
    public CellPossibilities(Cell cell, ReadOnlyBitSet16 possibilities)
    {
        Cell = cell;
        Possibilities = possibilities;
    }

    public CellPossibilities(Cell cell, int possibility)
    {
        Cell = cell;
        var buffer = new ReadOnlyBitSet16();
        buffer += possibility;
        Possibilities = buffer;
    }
    
    public CellPossibilities(CellPossibility coord)
    {
        Cell = new Cell(coord.Row, coord.Column);
        var buffer = new ReadOnlyBitSet16();
        buffer += coord.Possibility;
        Possibilities = buffer;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CellPossibilities cp) return false;
        return Cell == cp.Cell && Possibilities.Equals(cp.Possibilities);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Cell.GetHashCode(), Possibilities.GetHashCode());
    }

    public override string ToString()
    {
        return $"{Cell} => {Possibilities}";
    }
}

public readonly struct MiniGrid
{
    public MiniGrid(int miniRow, int miniColumn)
    {
        MiniRow = miniRow;
        MiniColumn = miniColumn;
    }

    public int MiniRow { get; }
    public int MiniColumn { get; }
}