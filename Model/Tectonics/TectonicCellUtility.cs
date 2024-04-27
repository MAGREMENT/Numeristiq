using System;
using System.Collections.Generic;
using Model.Utility;

namespace Model.Tectonics;

public static class TectonicCellUtility
{
    public static bool AreNeighbors(Cell c1, Cell c2)
        => c1 != c2 && Math.Abs(c1.Row - c2.Row) <= 1 && Math.Abs(c1.Column - c2.Column) <= 1;

    public static bool AreAdjacent(Cell c1, Cell c2)
    {
        if (c1 == c2) return false;
        var rowDiff = Math.Abs(c1.Row - c2.Row);
        var colDiff = Math.Abs(c1.Column - c2.Column);
        
        return (rowDiff == 1 && colDiff == 0) || (rowDiff == 0 && colDiff == 1);
    }

    public static bool AreAdjacent(IZone z1, IZone z2)
    {
        foreach (var c1 in z1)
        {
            if (AreAdjacent(z2, c1)) return true;
        }

        return false;
    }

    public static bool AreAdjacent(IZone zone, Cell cell)
    {
        foreach (var c in zone)
        {
            if (AreAdjacent(c, cell)) return true;
        }

        return false;
    }

    public static bool AreAdjacent(IReadOnlyList<Cell> cells, Cell cell)
    {
        foreach (var c in cells)
        {
            if (AreAdjacent(c, cell)) return true;
        }

        return false;
    }

    public static IEnumerable<Cell[]> DivideInAdjacentCells(List<Cell> cells)
    {
        List<Cell> current = new();

        while (cells.Count > 0)
        {
            current.Add(cells[^1]);
            cells.RemoveAt(cells.Count - 1);
            var added = true;
            
            while (added)
            {
                added = false;
                for (int i = cells.Count - 1; i >= 0; i--)
                {
                    if (!AreAdjacent(current, cells[i])) continue;

                    current.Add(cells[i]);
                    cells.RemoveAt(i);
                    added = true;
                }
            }

            yield return current.ToArray();
            current.Clear();
        }
    }

    public static IEnumerable<Cell> GetNeighbors(Cell cell, int rowCount, int colCount)
    {
        return GetNeighbors(cell.Row, cell.Column, rowCount, colCount);
    }
    
    public static IEnumerable<Cell> GetNeighbors(int row, int col, int rowCount, int colCount)
    {
        if (row > 0)
        {
            yield return new Cell(row - 1, col);
            if (col > 0) yield return new Cell(row - 1, col - 1);
        }

        if (col > 0)
        {
            yield return new Cell(row, col - 1);
            if (row < rowCount - 1) yield return new Cell(row + 1, col - 1);
        }

        if (row < rowCount - 1)
        {
            yield return new Cell(row + 1, col);
            if (col < colCount - 1) yield return new Cell(row + 1, col + 1);
        }

        if (col < colCount - 1)
        {
            yield return new Cell(row, col + 1);
            if (row > 0) yield return new Cell(row - 1, col + 1);
        }
    }
    
    public static IEnumerable<Cell> SharedNeighboringCells(IReadOnlyTectonic tectonic, IReadOnlyList<Cell> cells)
    {
        if (cells.Count == 0) yield break;

        foreach (var neighbor in GetNeighbors(cells[0], tectonic.RowCount, tectonic.ColumnCount))
        {
            bool ok = true;

            for (int i = 1; i < cells.Count; i++)
            {
                if (!AreNeighbors(neighbor, cells[i]) || cells[i] == neighbor)
                {
                    ok = false;
                    break;
                }
            }

            if (ok) yield return neighbor;
        }
    }
    
    public static IEnumerable<Cell> SharedNeighboringCells(IReadOnlyTectonic tectonic, Cell one, Cell two)
    {
        foreach (var neighbor in GetNeighbors(one, tectonic.RowCount, tectonic.ColumnCount))
        {
            if (AreNeighbors(two, neighbor)) yield return neighbor;
        }
    }

    public static IEnumerable<Cell> SharedSeenCells(IReadOnlyTectonic tectonic, Cell one, Cell two) //TODO to non-repeating cells
    {
        foreach (var neighbor in GetNeighbors(one, tectonic.RowCount, tectonic.ColumnCount))
        {
            if (AreNeighbors(neighbor, two)) yield return neighbor;
        }

        var z1 = tectonic.GetZone(one);
        var z2 = tectonic.GetZone(two);

        if (z1.Equals(z2))
        {
            foreach (var cell in z1)
            {
                if (cell != one && cell != two) yield return cell;
            }
        }
        else
        {
            foreach (var cell in z1)
            {
                if (cell != one && AreNeighbors(cell, two)) yield return cell;
            }
        
            foreach (var cell in z2)
            {
                if (cell != two && AreNeighbors(cell, one)) yield return cell;
            }
        }
    }

    public static IEnumerable<Cell> SharedSeenCells(IReadOnlyTectonic tectonic, IReadOnlyList<Cell> cells)
    {
        if (cells.Count == 0) yield break;
        if (cells.Count == 1) yield break; //TODO

        foreach (var cell in SharedSeenCells(tectonic, cells[0], cells[1]))
        {
            bool ok = true;
            
            for (int i = 2; i < cells.Count; i++)
            {
                if (cells[i] == cell || (!AreNeighbors(cells[i], cell) && !tectonic.GetZone(cells[i]).Contains(cell)))
                {
                    ok = false;
                    break;
                }
            }

            if (ok) yield return cell;
        }
    }

    public static IEnumerable<CellPossibility> SharedSeenPossibilities(ITectonicStrategyUser strategyUser,
        CellPossibility cp1, CellPossibility cp2)
    {
        var c1 = cp1.ToCell();
        var c2 = cp2.ToCell();
        if (cp1.Possibility == cp2.Possibility)
        {
            foreach (var cell in SharedSeenCells(strategyUser.Tectonic, c1, c2))
            {
                yield return new CellPossibility(cell, cp1.Possibility);
            }
        }
        else if (c1 == c2)
        {
            foreach (var p in strategyUser.PossibilitiesAt(c1).EnumeratePossibilities())
            {
                if (p != cp1.Possibility && p != cp2.Possibility)
                    yield return new CellPossibility(c1, p);
            }
        }
    }
}