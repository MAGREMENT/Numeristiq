using System;
using System.Collections.Generic;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil.LinkGraph;
using Model.Solver.StrategiesUtil.SharedCellSearcher;

namespace Model.Solver.StrategiesUtil;

public static class Cells
{
    private static readonly ISharedSeenCellSearcher Searcher = new InCommonFindSearcher();
    
    public static bool ShareAUnit(int row1, int col1, int row2, int col2)
    {
        return row1 == row2 || col1 == col2 ||
               (row1 / 3 == row2 / 3
                && col1 / 3 == col2 / 3);
    }

    public static List<Cell> SeenCells(Cell cell)
    {
        List<Cell> result = new();
        
        for (int i = 0; i < 9; i++)
        {
            if (i != cell.Row) result.Add(new Cell(i, cell.Col));
            if (i != cell.Col) result.Add(new Cell(cell.Row, i));
        }

        var startRow = cell.Row / 3 * 3;
        var startCol = cell.Col / 3 * 3;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var row = startRow + i;
                var col = startCol + i;

                if (row != cell.Row && col != cell.Col) result.Add(new Cell(row, col));
            }
        }

        return result;
    }
    
    public static IEnumerable<Cell> SharedSeenCells(int row1, int col1, int row2, int col2) //TODO "AsList"
    {
        return Searcher.SharedSeenCells(row1, col1, row2, col2);
    }

    public static IEnumerable<Cell> SharedSeenCells(Cell one, Cell two, params Cell[] others)
    {
        foreach (var coord in one.SharedSeenCells(two))
        {
            bool ok = true;
            foreach (var other in others)
            {
                if (!other.ShareAUnit(coord) || other == coord)
                {
                    ok = false;
                    break;
                }
            }

            if (ok) yield return coord;
        }
    }
    
    public static List<Cell> SharedSeenCells(List<Cell> list)
    {
        if (list.Count == 0) return new List<Cell>();
        if (list.Count == 1) return SeenCells(list[^1]);

        var result = new List<Cell>();
        foreach (var coord in list[0].SharedSeenCells(list[1]))
        {
            bool ok = true;
            for (int i = 2; i < list.Count; i++)
            {
                if (!list[i].ShareAUnit(coord) || list[i] == coord)
                {
                    ok = false;
                    break;
                }
            }

            if (ok) result.Add(coord);
        }

        return result;
    }

    public static IEnumerable<Cell> SharedSeenEmptyCells(IStrategyManager strategyManager, int row1, int col1, int row2, int col2)
    {
        return Searcher.SharedSeenEmptyCells(strategyManager, row1, col1, row2, col2);
    }
}

public readonly struct Cell
{
    public int Row { get; }
    public int Col { get; }


    public Cell(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public bool ShareAUnit(Cell coord)
    {
        return Cells.ShareAUnit(Row, Col, coord.Row, coord.Col);
    }
    
    public bool ShareAUnitWithAll(List<Cell> coordinates)
    {
        foreach (var coord in coordinates)
        {
            if (!ShareAUnit(coord)) return false;
        }

        return true;
    }

    public IEnumerable<Cell> SharedSeenCells(Cell coord)
    {
        return Cells.SharedSeenCells(Row, Col, coord.Row, coord.Col);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Col);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Cell coord) return false;
        return Row == coord.Row && Col == coord.Col;
    }

    public override string ToString()
    {
        return $"[{Row + 1}, {Col + 1}]";
    }

    public static bool operator ==(Cell left, Cell right)
    {
        return left.Row == right.Row && left.Col == right.Col;
    }

    public static bool operator !=(Cell left, Cell right)
    {
        return !(left == right);
    }
}

public interface ICellPossibility
{
    public int Possibility { get; }
    public int Row { get; }
    public int Col { get; } 
}

public readonly struct CellPossibility : ILinkGraphElement, ICellPossibility
{
    public int Possibility { get; }
    public int Row { get; }
    public int Col { get; }

    public CellPossibility(int row, int col, int possibility)
    {
        Possibility = possibility;
        Row = row;
        Col = col;
    }

    public CellPossibility(Cell coord, int possibility)
    {
        Possibility = possibility;
        Row = coord.Row;
        Col = coord.Col;
    }
    
    public bool ShareAUnit(CellPossibility coord)
    {
        return Cells.ShareAUnit(Row, Col, coord.Row, coord.Col);
    }
    
    public bool ShareAUnit(Cell coord)
    {
        return Cells.ShareAUnit(Row, Col, coord.Row, coord.Col);
    }

    public IEnumerable<Cell> SharedSeenCells(CellPossibility coord)
    {
        return Cells.SharedSeenCells(Row, Col, coord.Row, coord.Col);
    }
    
    public IEnumerable<Cell> SharedSeenCells(Cell coord)
    {
        return Cells.SharedSeenCells(Row, Col, coord.Row, coord.Col);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Possibility, Row, Col);
    }

    public override bool Equals(object? obj)
    {
        return (obj is CellPossibility cp && cp == this) ||
               (obj is ICellPossibility icp && icp.Possibility == Possibility && icp.Row == Row && icp.Col == Col);
    }

    public override string ToString()
    {
        return $"{Possibility}[{Row + 1}, {Col + 1}]";
    }

    public CellPossibilities[] EveryCellPossibilities()
    {
        return new[] { new CellPossibilities(this) };
    }

    public Cell[] EveryCell()
    {
        return new Cell[] { new(Row, Col) };
    }

    public IPossibilities EveryPossibilities()
    {
        var result = IPossibilities.NewEmpty();
        result.Add(Possibility);
        return result;
    }

    public Cell ToCell()
    {
        return new Cell(Row, Col);
    }

    public static bool operator ==(CellPossibility left, CellPossibility right)
    {
        return left.Possibility == right.Possibility && left.Row == right.Row && left.Col == right.Col;
    }

    public static bool operator !=(CellPossibility left, CellPossibility right)
    {
        return !(left == right);
    }
}

public class CellPossibilities
{
    public Cell Cell { get; }
    public IReadOnlyPossibilities Possibilities { get; }
    
    public CellPossibilities(Cell cell, IReadOnlyPossibilities possibilities)
    {
        Cell = cell;
        Possibilities = possibilities;
    }

    public CellPossibilities(Cell cell, int possibility)
    {
        Cell = cell;
        var buffer = IPossibilities.NewEmpty();
        buffer.Add(possibility);
        Possibilities = buffer;
    }
    
    public CellPossibilities(CellPossibility coord)
    {
        Cell = new Cell(coord.Row, coord.Col);
        var buffer = IPossibilities.NewEmpty();
        buffer.Add(coord.Possibility);
        Possibilities = buffer;
    }

    public CellPossibility[] ToCellPossibility()
    {
        var result = new CellPossibility[Possibilities.Count];

        var cursor = 0;
        foreach (var possibility in Possibilities)
        {
            result[cursor] = new CellPossibility(Cell.Row, Cell.Col, possibility);
            cursor++;
        }

        return result;
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